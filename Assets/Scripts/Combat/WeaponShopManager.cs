using System;
using System.Collections.Generic;
using UnityEngine;
using Roguelite.SaveSystem;
using Roguelite.Player;
using Roguelite.UpgradeSystem;

namespace Roguelite.Combat
{
    /// <summary>
    /// Singleton Quản lý toàn bộ cửa hàng vũ khí, xử lý mở khóa theo thành tựu (Kills, Runs, Rooms),
    /// thanh toán mua vũ khí bằng Vàng, và trang bị vũ khí đồng bộ với SaveData.
    /// </summary>
    public class WeaponShopManager : MonoBehaviour
    {
        private static WeaponShopManager instance;
        public static WeaponShopManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<WeaponShopManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("[WeaponShopManager]");
                        instance = go.AddComponent<WeaponShopManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        [Header("Database Configuration")]
        [Tooltip("Database tập hợp tất cả vũ khí trong game.")]
        [SerializeField] private WeaponDatabase database;

        // Events
        public static event Action<WeaponData> OnWeaponPurchased;
        public static event Action<WeaponData> OnWeaponEquipped;
        public static event Action OnWeaponUnlockStateChanged;

        public WeaponDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = GetOrLoadWeaponDatabase();
                }
                return database;
            }
            set => database = value;
        }

        public static WeaponDatabase GetOrLoadWeaponDatabase()
        {
            WeaponDatabase db = Resources.Load<WeaponDatabase>("WeaponDatabase");
            if (db == null)
            {
                var dbs = Resources.FindObjectsOfTypeAll<WeaponDatabase>();
                if (dbs != null && dbs.Length > 0) db = dbs[0];
            }
#if UNITY_EDITOR
            if (db == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponDatabase");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    db = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponDatabase>(path);
                }
            }
#endif
            return db;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureDefaultWeaponsUnlocked();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Tự động kiểm tra và thêm các vũ khí mặc định (isDefaultUnlocked) vào SaveData nếu chưa có.
        /// </summary>
        public void EnsureDefaultWeaponsUnlocked()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            if (wData == null) return;

            bool changed = false;
            if (database != null && database.AllWeapons != null)
            {
                foreach (var weapon in database.AllWeapons)
                {
                    if (weapon == null) continue;
                    string id = GetWeaponId(weapon);

                    if (weapon.IsDefaultUnlocked)
                    {
                        if (!wData.unlockedWeaponIds.Contains(id))
                        {
                            wData.unlockedWeaponIds.Add(id);
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                SaveManager.Instance.TriggerAutoSave(0.5f);
            }
        }

        // =====================================================================
        //  PUBLIC HELPER & QUERY METHODS
        // =====================================================================

        public string GetWeaponId(WeaponData weapon)
        {
            if (weapon == null) return string.Empty;
            return string.IsNullOrEmpty(weapon.WeaponId) ? weapon.name : weapon.WeaponId;
        }

        /// <summary>
        /// Lấy số lượng vũ khí Support đang trang bị (tối đa 3).
        /// </summary>
        public int GetEquippedCount()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return 0;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            return wData != null && wData.equippedWeaponIds != null ? wData.equippedWeaponIds.Count : 0;
        }

        /// <summary>
        /// Kiểm tra xem 1 vũ khí đã được mở khóa (đã mua) hay chưa.
        /// </summary>
        public bool IsWeaponUnlocked(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return false;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;

            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            return wData != null && wData.unlockedWeaponIds.Contains(weaponId);
        }

        public bool IsWeaponUnlocked(WeaponData weapon)
        {
            if (weapon == null) return false;
            return IsWeaponUnlocked(GetWeaponId(weapon));
        }

        /// <summary>
        /// Kiểm tra xem vũ khí này có đang nằm trong danh sách 3 vũ khí Support trang bị lượt này không.
        /// </summary>
        public bool IsWeaponEquipped(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;

            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            return wData != null && wData.equippedWeaponIds != null && wData.equippedWeaponIds.Contains(GetWeaponId(weapon));
        }

        /// <summary>
        /// Kiểm tra xem người chơi đã đạt đủ tất cả điều kiện (quái diệt, lượt run, cấp phòng) để mở khóa mua vũ khí này chưa.
        /// </summary>
        public bool IsRequirementMet(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;

            return weapon.IsRequirementMet(SaveManager.Instance.CurrentSaveData.progressData);
        }

        /// <summary>
        /// Kiểm tra xem người chơi có đủ Vàng để mua vũ khí này không.
        /// </summary>
        public bool CanAffordWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;

            int currentGold = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
            return currentGold >= weapon.Price;
        }

        // =====================================================================
        //  TRANSACTION & ACTION METHODS
        // =====================================================================

        /// <summary>
        /// Thực hiện mua vũ khí trong cửa hàng.
        /// Trừ vàng -> Thêm vũ khí vào unlockedWeaponIds -> Lưu game -> Phát Event.
        /// </summary>
        public bool TryPurchaseWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;
            string weaponId = GetWeaponId(weapon);

            if (IsWeaponUnlocked(weaponId)) return false;

            if (!IsRequirementMet(weapon))
            {
                Debug.LogWarning($"[WeaponShopManager] Chưa đạt đủ điều kiện mở khóa cho '{weapon.WeaponName}'!");
                return false;
            }

            bool spentSuccess = PermanentUpgradeManager.Instance != null
                ? PermanentUpgradeManager.Instance.SpendCurrency(weapon.Price)
                : DirectSpendCurrency(weapon.Price);

            if (!spentSuccess)
            {
                Debug.LogWarning($"[WeaponShopManager] Không đủ tiền mua '{weapon.WeaponName}' (Cần {weapon.Price} Gold)!");
                return false;
            }

            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            if (!wData.unlockedWeaponIds.Contains(weaponId))
            {
                wData.unlockedWeaponIds.Add(weaponId);
            }

            SaveManager.Instance.TriggerAutoSave(0.5f);
            Debug.Log($"[WeaponShopManager] 🎉 Mua thành công vũ khí '{weapon.WeaponName}'!");

            OnWeaponPurchased?.Invoke(weapon);
            OnWeaponUnlockStateChanged?.Invoke();
            return true;
        }

        private bool DirectSpendCurrency(int amount)
        {
            if (amount <= 0) return true;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;

            var progress = SaveManager.Instance.CurrentSaveData.progressData;
            if (progress.totalCurrency >= amount)
            {
                progress.totalCurrency -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Bật/Tắt trang bị vũ khí support (Tối đa 3 vũ khí trong 1 lượt run).
        /// </summary>
        public bool ToggleEquipSupportWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (IsWeaponEquipped(weapon))
            {
                return UnequipSupportWeapon(weapon);
            }
            else
            {
                return EquipSupportWeapon(weapon);
            }
        }

        /// <summary>
        /// Trang bị 1 vũ khí Support (Alias tương thích ngược).
        /// </summary>
        public bool EquipWeapon(WeaponData weapon)
        {
            return EquipSupportWeapon(weapon);
        }

        /// <summary>
        /// Trang bị 1 vũ khí Support vào slot lượt run hiện tại (tối đa 3 slots).
        /// </summary>
        public bool EquipSupportWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;

            string weaponId = GetWeaponId(weapon);
            if (!IsWeaponUnlocked(weaponId))
            {
                Debug.LogWarning($"[WeaponShopManager] Vũ khí '{weapon.WeaponName}' chưa được mua! Không thể trang bị.");
                return false;
            }

            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;

            if (wData.equippedWeaponIds.Contains(weaponId)) return true;

            if (wData.equippedWeaponIds.Count >= WeaponUnlockData.MAX_EQUIPPED_SLOTS)
            {
                Debug.LogWarning($"[WeaponShopManager] ⚠️ Đã đạt tối đa {WeaponUnlockData.MAX_EQUIPPED_SLOTS} vũ khí Support trang bị trong lượt run này!");
                return false;
            }

            wData.equippedWeaponIds.Add(weaponId);
            SaveManager.Instance.TriggerAutoSave(0.3f);

            ApplyEquippedWeaponBuffsToActivePlayer();

            Debug.Log($"[WeaponShopManager] 🗡️ Đã trang bị Support Weapon: '{weapon.WeaponName}' (Slot {wData.equippedWeaponIds.Count}/{WeaponUnlockData.MAX_EQUIPPED_SLOTS})");
            OnWeaponEquipped?.Invoke(weapon);
            OnWeaponUnlockStateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Gỡ bỏ 1 vũ khí Support khỏi slot lượt run hiện tại.
        /// </summary>
        public bool UnequipSupportWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;

            string weaponId = GetWeaponId(weapon);
            if (wData.equippedWeaponIds.Remove(weaponId))
            {
                SaveManager.Instance.TriggerAutoSave(0.3f);
                ApplyEquippedWeaponBuffsToActivePlayer();

                Debug.Log($"[WeaponShopManager] 🛡️ Đã gỡ bỏ Support Weapon: '{weapon.WeaponName}'");
                OnWeaponUnlockStateChanged?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset toàn bộ loadout vũ khí trang bị về trống khi bắt đầu một lượt run mới.
        /// </summary>
        public void ResetEquippedWeaponsForNewRun()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;

            if (wData.equippedWeaponIds.Count > 0)
            {
                wData.equippedWeaponIds.Clear();
                SaveManager.Instance.TriggerAutoSave(0.2f);
                Debug.Log("[WeaponShopManager] 🔄 Đã reset loadout trang bị vũ khí cho lượt run mới!");
                OnWeaponUnlockStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Tính tổng sát thương bổ trợ từ tất cả vũ khí support đang trang bị.
        /// </summary>
        public float GetTotalSupportDamage()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return 0f;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            if (wData == null || wData.equippedWeaponIds == null) return 0f;

            float totalDamage = 0f;
            foreach (string id in wData.equippedWeaponIds)
            {
                WeaponData weapon = Database != null ? Database.GetWeaponById(id) : null;
                if (weapon != null)
                {
                    totalDamage += weapon.Damage;
                }
            }
            return totalDamage;
        }

        /// <summary>
        /// Tính tổng lực hất bổ trợ từ tất cả vũ khí support đang trang bị.
        /// </summary>
        public Vector2 GetTotalSupportKnockback()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return Vector2.zero;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            if (wData == null || wData.equippedWeaponIds == null) return Vector2.zero;

            Vector2 totalKnockback = Vector2.zero;
            foreach (string id in wData.equippedWeaponIds)
            {
                WeaponData weapon = Database != null ? Database.GetWeaponById(id) : null;
                if (weapon != null)
                {
                    totalKnockback += weapon.Knockback;
                }
            }
            return totalKnockback;
        }

        /// <summary>
        /// Tính toán tổng cộng dồn các chỉ số Support Buff từ tối đa 3 vũ khí trang bị và áp dụng lên Player.
        /// </summary>
        public void ApplyEquippedWeaponBuffs(GameObject player)
        {
            if (player == null) return;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return;

            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            if (wData == null || wData.equippedWeaponIds == null) return;

            float bonusDamage = 0f;
            float totalKnockbackX = 0f;
            float totalKnockbackY = 0f;

            foreach (string id in wData.equippedWeaponIds)
            {
                WeaponData weapon = database != null ? database.GetWeaponById(id) : null;
                if (weapon != null)
                {
                    bonusDamage += weapon.Damage;
                    totalKnockbackX += weapon.Knockback.x;
                    totalKnockbackY += weapon.Knockback.y;
                }
            }

            // Áp dụng lên Attack components của Player
            Attack[] attacks = player.GetComponentsInChildren<Attack>(true);
            foreach (Attack attack in attacks)
            {
                if (attack == null) continue;
                attack.AttackDamage += bonusDamage;
                attack.Knockback += new Vector2(totalKnockbackX, totalKnockbackY);
            }

            Debug.Log($"[WeaponShopManager] ✨ Đã áp dụng Support Buffs từ {wData.equippedWeaponIds.Count} vũ khí trang bị lên Player! (Bonus Damage: +{bonusDamage})");
        }

        public void ApplyEquippedWeaponBuffsToActivePlayer()
        {
            WeaponManager playerWM = FindObjectOfType<WeaponManager>();
            if (playerWM != null)
            {
                ApplyEquippedWeaponBuffs(playerWM.gameObject);
            }
        }

        // =====================================================================
        //  DEBUG / CHEAT HELPERS
        // =====================================================================

        /// <summary>
        /// Cheat mở khóa toàn bộ vũ khí trong database.
        /// </summary>
        public void UnlockAllWeapons()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;

            if (database != null && database.AllWeapons != null)
            {
                foreach (var weapon in database.AllWeapons)
                {
                    if (weapon == null) continue;
                    string id = GetWeaponId(weapon);
                    if (!wData.unlockedWeaponIds.Contains(id))
                    {
                        wData.unlockedWeaponIds.Add(id);
                    }
                }
            }

            SaveManager.Instance.TriggerAutoSave(0.5f);
            Debug.Log("[WeaponShopManager] [CHEAT] Đã mở khóa toàn bộ vũ khí!");
            OnWeaponUnlockStateChanged?.Invoke();
        }

        /// <summary>
        /// Cheat reset lại trạng thái unlock vũ khí về ban đầu.
        /// </summary>
        public void ResetWeaponUnlocks()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return;
            var wData = SaveManager.Instance.CurrentSaveData.weaponData;
            wData.unlockedWeaponIds.Clear();

            EnsureDefaultWeaponsUnlocked();
            OnWeaponUnlockStateChanged?.Invoke();
            Debug.Log("[WeaponShopManager] [CHEAT] Đã reset trạng thái mở khóa vũ khí!");
        }
    }
}
