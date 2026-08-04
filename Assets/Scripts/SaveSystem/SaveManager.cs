using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Singleton quản lý toàn bộ luồng lưu và tải dữ liệu tiến trình & cài đặt (Hỗ trợ 3 Save Slots).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public static readonly int CURRENT_SAVE_VERSION = 2;
        public static readonly int CURRENT_SETTING_VERSION = 1;

        public const int MIN_SLOT_INDEX = 1;
        public const int MAX_SLOT_INDEX = 3;

        private const string LEGACY_SAVE_FILE_NAME = "save_data.json";
        private const string SETTING_FILE_NAME = "settings.json";

        public SaveData CurrentSaveData { get; private set; }
        public SettingData CurrentSettingData { get; private set; }

        public int CurrentSlotIndex { get; private set; } = 1;

        private string saveFilePath;
        private string saveBackupPath;
        private string settingFilePath;

        private bool isSaving = false;
        private Coroutine autoSaveDebounceCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettingData(); // Nạp setting trước để biết lastActiveSlotIndex
                SetCurrentSlot(CurrentSettingData.lastActiveSlotIndex, autoLoad: false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePaths()
        {
            string basePath = Application.persistentDataPath;
            settingFilePath = Path.Combine(basePath, SETTING_FILE_NAME);

            string slotFileName = GetSlotFileName(CurrentSlotIndex);
            saveFilePath = Path.Combine(basePath, slotFileName);
            saveBackupPath = Path.Combine(basePath, slotFileName + ".bak");
        }

        public static string GetSlotFileName(int slotIndex)
        {
            int validSlot = Mathf.Clamp(slotIndex, MIN_SLOT_INDEX, MAX_SLOT_INDEX);
            return $"save_data_slot_{validSlot}.json";
        }

        public string GetSaveFilePath() => saveFilePath;
        public string GetSettingFilePath() => settingFilePath;

        // --- SLOT MANAGEMENT ---

        /// <summary>
        /// Chuyển đổi ô lưu hiện tại (từ 1 đến 3).
        /// </summary>
        public void SetCurrentSlot(int slotIndex, bool autoLoad = true)
        {
            CurrentSlotIndex = Mathf.Clamp(slotIndex, MIN_SLOT_INDEX, MAX_SLOT_INDEX);

            if (CurrentSettingData != null && CurrentSettingData.lastActiveSlotIndex != CurrentSlotIndex)
            {
                CurrentSettingData.lastActiveSlotIndex = CurrentSlotIndex;
                SaveSettingData();
            }

            InitializePaths();

            if (autoLoad)
            {
                LoadFromDisk();
            }
        }

        /// <summary>
        /// Kiểm tra Slot X có tồn tại file save hay chưa.
        /// </summary>
        public bool DoesSlotExist(int slotIndex)
        {
            string path = Path.Combine(Application.persistentDataPath, GetSlotFileName(slotIndex));
            return File.Exists(path);
        }

        /// <summary>
        /// Đọc thông tin xem trước dữ liệu của Slot X (dùng cho UI chọn Slot).
        /// </summary>
        public SaveData LoadSlotData(int slotIndex)
        {
            string path = Path.Combine(Application.persistentDataPath, GetSlotFileName(slotIndex));
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Lỗi đọc xem trước Slot {slotIndex}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Xóa file save và backup của Slot X để reset ô lưu.
        /// </summary>
        public void DeleteSlot(int slotIndex)
        {
            string slotFileName = GetSlotFileName(slotIndex);
            string path = Path.Combine(Application.persistentDataPath, slotFileName);
            string backupPath = Path.Combine(Application.persistentDataPath, slotFileName + ".bak");

            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(backupPath)) File.Delete(backupPath);

            Debug.Log($"[SaveManager] Đã xóa thành công Slot {slotIndex}.");

            if (slotIndex == CurrentSlotIndex)
            {
                CurrentSaveData = CreateDefaultSaveData(CurrentSlotIndex);
            }
        }

        // --- PROGRESS SAVE / LOAD ---

        /// <summary>
        /// Tải SaveData của Slot hiện tại từ đĩa. Nếu chưa có hoặc lỗi JSON, khởi tạo dữ liệu mặc định.
        /// </summary>
        public SaveData LoadFromDisk()
        {
            InitializePaths();

            // Kiểm tra di cư file save_data.json legacy (V1 -> Slot 1 V2)
            CheckAndMigrateLegacyFile();

            if (!File.Exists(saveFilePath))
            {
                Debug.Log($"[SaveManager] Chưa có file save cho Slot {CurrentSlotIndex}. Khởi tạo dữ liệu mặc định.");
                CurrentSaveData = CreateDefaultSaveData(CurrentSlotIndex);
                SaveToDiskSync();
                return CurrentSaveData;
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

                if (loadedData == null)
                {
                    throw new Exception("Dữ liệu JSON giải mã ra null.");
                }

                // Migration check
                if (loadedData.saveVersion < CURRENT_SAVE_VERSION)
                {
                    loadedData = MigrateSaveData(loadedData);
                }

                loadedData.slotIndex = CurrentSlotIndex;
                CurrentSaveData = loadedData;
                Debug.Log($"[SaveManager] Tải SaveData Slot {CurrentSlotIndex} thành công! Version: {CurrentSaveData.saveVersion}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Lỗi đọc file SaveData Slot {CurrentSlotIndex} ({ex.Message}). Thử phục hồi từ file backup...");
                if (!RestoreFromBackup())
                {
                    Debug.LogWarning($"[SaveManager] Cả file chính và backup của Slot {CurrentSlotIndex} đều hỏng. Tạo mới SaveData mặc định.");
                    CurrentSaveData = CreateDefaultSaveData(CurrentSlotIndex);
                    SaveToDiskSync();
                }
            }

            return CurrentSaveData;
        }

        /// <summary>
        /// Lưu đồng bộ dữ liệu SaveData hiện tại xuống đĩa kèm cơ chế backup.
        /// </summary>
        public void SaveToDiskSync()
        {
            if (CurrentSaveData == null)
            {
                CurrentSaveData = CreateDefaultSaveData(CurrentSlotIndex);
            }

            try
            {
                InitializePaths();
                CurrentSaveData.slotIndex = CurrentSlotIndex;
                CurrentSaveData.saveVersion = CURRENT_SAVE_VERSION;
                CurrentSaveData.lastSavedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string json = JsonUtility.ToJson(CurrentSaveData, true);

                // Backup file cũ trước khi ghi đè
                if (File.Exists(saveFilePath))
                {
                    File.Copy(saveFilePath, saveBackupPath, overwrite: true);
                }

                File.WriteAllText(saveFilePath, json);
                Debug.Log($"[SaveManager] Lưu SaveData Slot {CurrentSlotIndex} thành công xuống đĩa.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Lỗi ghi đè SaveData Slot {CurrentSlotIndex} xuống đĩa: {ex.Message}");
            }
        }

        // --- SETTINGS SAVE / LOAD ---

        public SettingData LoadSettingData()
        {
            InitializePaths();

            if (!File.Exists(settingFilePath))
            {
                Debug.Log("[SaveManager] Chưa có file settings.json. Tạo cài đặt mặc định.");
                CurrentSettingData = new SettingData();
                SaveSettingData();
                return CurrentSettingData;
            }

            try
            {
                string json = File.ReadAllText(settingFilePath);
                SettingData loadedSetting = JsonUtility.FromJson<SettingData>(json);

                if (loadedSetting == null)
                {
                    throw new Exception("Setting JSON giải mã ra null.");
                }

                CurrentSettingData = loadedSetting;
                CurrentSlotIndex = Mathf.Clamp(CurrentSettingData.lastActiveSlotIndex, MIN_SLOT_INDEX, MAX_SLOT_INDEX);
                Debug.Log("[SaveManager] Tải SettingData thành công.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Lỗi đọc file SettingData ({ex.Message}). Fallback về mặc định.");
                CurrentSettingData = new SettingData();
                SaveSettingData();
            }

            return CurrentSettingData;
        }

        public void SaveSettingData()
        {
            if (CurrentSettingData == null)
            {
                CurrentSettingData = new SettingData();
            }

            try
            {
                InitializePaths();
                CurrentSettingData.lastActiveSlotIndex = CurrentSlotIndex;
                string json = JsonUtility.ToJson(CurrentSettingData, true);
                File.WriteAllText(settingFilePath, json);
                Debug.Log("[SaveManager] Lưu SettingData thành công.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Lỗi ghi SettingData: {ex.Message}");
            }
        }

        // --- COROUTINE SAVE & DEBOUNCE ---

        public void SaveToDiskAsync()
        {
            if (isSaving) return;
            StartCoroutine(SaveToDiskCoroutine());
        }

        private IEnumerator SaveToDiskCoroutine()
        {
            isSaving = true;
            yield return null;
            SaveToDiskSync();
            isSaving = false;
        }

        public void TriggerAutoSave(float delaySeconds = 1.5f)
        {
            if (autoSaveDebounceCoroutine != null)
            {
                StopCoroutine(autoSaveDebounceCoroutine);
            }

            autoSaveDebounceCoroutine = StartCoroutine(DebouncedAutoSaveCoroutine(delaySeconds));
        }

        private IEnumerator DebouncedAutoSaveCoroutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            SaveToDiskAsync();
            autoSaveDebounceCoroutine = null;
        }

        // --- UTILS & MIGRATION ---

        private SaveData CreateDefaultSaveData(int slot)
        {
            return new SaveData
            {
                saveVersion = CURRENT_SAVE_VERSION,
                slotIndex = slot,
                lastSavedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                progressData = new PlayerProgressData(),
                weaponData = new WeaponUnlockData(),
                abilityData = new AbilityUnlockData()
            };
        }

        private void CheckAndMigrateLegacyFile()
        {
            string legacyPath = Path.Combine(Application.persistentDataPath, LEGACY_SAVE_FILE_NAME);
            string slot1Path = Path.Combine(Application.persistentDataPath, GetSlotFileName(1));

            if (File.Exists(legacyPath) && !File.Exists(slot1Path))
            {
                try
                {
                    File.Move(legacyPath, slot1Path);
                    Debug.Log("[SaveManager] Đã di cư thành công file legacy save_data.json sang save_data_slot_1.json.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] Lỗi di cư file legacy: {ex.Message}");
                }
            }
        }

        private bool RestoreFromBackup()
        {
            if (!File.Exists(saveBackupPath)) return false;

            try
            {
                string backupJson = File.ReadAllText(saveBackupPath);
                SaveData backupData = JsonUtility.FromJson<SaveData>(backupJson);
                if (backupData != null)
                {
                    CurrentSaveData = backupData;
                    File.Copy(saveBackupPath, saveFilePath, overwrite: true);
                    Debug.Log($"[SaveManager] Phục hồi thành công dữ liệu từ file backup .bak của Slot {CurrentSlotIndex}!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Khôi phục từ backup thất bại: {ex.Message}");
            }
            return false;
        }

        private SaveData MigrateSaveData(SaveData oldData)
        {
            Debug.Log($"[SaveManager] Nâng cấp phiên bản SaveData từ V{oldData.saveVersion} lên V{CURRENT_SAVE_VERSION}");

            if (oldData.saveVersion < 2)
            {
                oldData.saveVersion = 2;
                if (oldData.slotIndex <= 0) oldData.slotIndex = CurrentSlotIndex;
            }

            return oldData;
        }

        // --- CONTEXT MENU TEST UTILS FOR UNITY EDITOR ---

        [ContextMenu("Test/Open Save Folder")]
        private void DebugOpenSaveFolder()
        {
            InitializePaths();
            string folder = Application.persistentDataPath;
            Debug.Log($"[SaveManager] Thư mục Save Path: {folder}");
            System.Diagnostics.Process.Start("explorer.exe", folder.Replace("/", "\\"));
        }

        [ContextMenu("Test/Switch to Slot 1")]
        private void DebugSelectSlot1() => SetCurrentSlot(1);

        [ContextMenu("Test/Switch to Slot 2")]
        private void DebugSelectSlot2() => SetCurrentSlot(2);

        [ContextMenu("Test/Switch to Slot 3")]
        private void DebugSelectSlot3() => SetCurrentSlot(3);

        [ContextMenu("Test/Save Data Sync")]
        private void DebugSaveDataSync() => SaveToDiskSync();

        [ContextMenu("Test/Load Data Sync")]
        private void DebugLoadDataSync() => LoadFromDisk();

        [ContextMenu("Test/Trigger AutoSave (Debounced)")]
        private void DebugTriggerAutoSave() => TriggerAutoSave(1.5f);

        [ContextMenu("Test/Add 100 Currency & Save")]
        private void DebugAddCurrencyAndSave()
        {
            if (CurrentSaveData == null) LoadFromDisk();
            CurrentSaveData.progressData.totalCurrency += 100;
            Debug.Log($"[SaveManager Test] Slot {CurrentSlotIndex} đã cộng 100 vàng. Tổng hiện tại: {CurrentSaveData.progressData.totalCurrency}");
            TriggerAutoSave(1.0f);
        }

        [ContextMenu("Test/Delete Current Slot")]
        private void DebugDeleteCurrentSlot() => DeleteSlot(CurrentSlotIndex);
    }
}
