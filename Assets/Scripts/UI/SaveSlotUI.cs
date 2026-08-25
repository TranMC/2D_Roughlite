using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Hiển thị thông tin của 1 ô lưu (Save Slot) trên giao diện chọn Slot.
    /// Gắn script này lên mỗi GameObject đại diện cho 1 slot trong panel chọn Slot.
    /// Hỗ trợ cả Auto Save slot (index 0) và Manual Save slots (index 1-3).
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("Slot Index (0 = AutoSave, 1-3 = Manual)")]
        [SerializeField] private int slotIndex = 1;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI slotTitleText;
        [SerializeField] private TextMeshProUGUI slotInfoText;
        [SerializeField] private Button slotButton;

        [Header("Optional Extended UI References (Tùy chọn)")]
        [SerializeField] private TextMeshProUGUI weaponText;
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        [SerializeField] private TextMeshProUGUI upgradesCountText;
        [SerializeField] private TextMeshProUGUI saveTimeText;

        private System.Action<int> onSlotSelected;

        /// <summary>Slot này có phải Auto Save không.</summary>
        public bool IsAutoSaveSlot => slotIndex == SaveManager.AUTOSAVE_SLOT_INDEX;

        /// <summary>Gán callback khi người chơi bấm chọn slot này.</summary>
        public void Setup(System.Action<int> callback)
        {
            onSlotSelected = callback;
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => onSlotSelected?.Invoke(slotIndex));
            }
            RefreshDisplay();
        }

        /// <summary>Cập nhật hiển thị thông tin slot (có dữ liệu hay trống).</summary>
        public void RefreshDisplay()
        {
            if (SaveManager.Instance == null)
            {
                SetEmptyDisplay();
                return;
            }

            bool exists = SaveManager.Instance.DoesSlotExist(slotIndex);

            if (!exists)
            {
                SetEmptyDisplay();
                return;
            }

            // Đọc dữ liệu xem trước (không ảnh hưởng slot đang active)
            SaveData previewData = SaveManager.Instance.LoadSlotData(slotIndex);

            if (previewData == null)
            {
                SetEmptyDisplay();
                return;
            }

            // Hiển thị tiêu đề
            if (slotTitleText != null)
            {
                slotTitleText.text = IsAutoSaveSlot ? "Auto Save" : $"Slot {slotIndex}";
            }

            string timeInfo = string.IsNullOrEmpty(previewData.lastSavedTime)
                ? "Chưa rõ"
                : previewData.lastSavedTime;

            int runs = previewData.progressData != null ? previewData.progressData.totalRunsPlayed : 0;
            int room = previewData.progressData != null ? previewData.progressData.highestRoomReached : 0;
            int gold = previewData.progressData != null ? previewData.progressData.totalCurrency : 0;
            int enemies = previewData.progressData != null ? previewData.progressData.totalEnemiesKilled : 0;

            int equippedCount = (previewData.weaponData != null && previewData.weaponData.equippedWeaponIds != null)
                ? previewData.weaponData.equippedWeaponIds.Count
                : 0;

            string weapon = equippedCount > 0 ? $"{equippedCount}/3 Support" : "Trống (0/3)";

            int upgradesCount = (previewData.abilityData != null && previewData.abilityData.abilityLevels != null)
                ? previewData.abilityData.abilityLevels.Count
                : 0;

            // Cập nhật các UI phụ nếu được gán trong Inspector
            if (weaponText != null) weaponText.text = $"Vũ khí: {weapon}";
            if (enemiesKilledText != null) enemiesKilledText.text = $"Diệt quái: {enemies}";
            if (upgradesCountText != null) upgradesCountText.text = $"Nâng cấp: {upgradesCount}";
            if (saveTimeText != null) saveTimeText.text = $"Lưu lúc: {timeInfo}";

            // Hiển thị tổng hợp dạng Rich Text lên slotInfoText với mã màu tương phản cao (High-Contrast)
            if (slotInfoText != null)
            {
                slotInfoText.text = $"<color=#222222>Run:</color> <color=#B85C00><b>{runs}</b></color> | <color=#222222>Phòng:</color> <color=#15803D><b>{room}</b></color> | <color=#222222>Vàng:</color> <color=#B45309><b>{gold}</b></color>\n" +
                                    $"<color=#222222>Quái diệt:</color> <color=#B91C1C><b>{enemies}</b></color> | <color=#222222>Vũ khí:</color> <color=#0369A1><b>{weapon}</b></color>\n" +
                                    $"<size=85%><color=#555555>Lưu lúc: {timeInfo}</color></size>";
            }
        }

        private string FormatWeaponName(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return "Mặc định";
            if (weaponId.Equals("sword_starter", System.StringComparison.OrdinalIgnoreCase)) return "Kiếm Thép";
            string formatted = weaponId.Replace("_", " ");
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted);
        }

        /// <summary>Hiển thị trạng thái trống khi slot chưa có dữ liệu.</summary>
        private void SetEmptyDisplay()
        {
            if (slotTitleText != null)
            {
                slotTitleText.text = IsAutoSaveSlot ? "Auto Save" : $"Slot {slotIndex}";
            }
            if (slotInfoText != null)
            {
                slotInfoText.text = "<color=#666666><b>[TRỐNG - CHƯA CÓ DỮ LIỆU]</b></color>";
            }
            if (weaponText != null) weaponText.text = "Vũ khí: --";
            if (enemiesKilledText != null) enemiesKilledText.text = "Diệt quái: 0";
            if (upgradesCountText != null) upgradesCountText.text = "Nâng cấp: 0";
            if (saveTimeText != null) saveTimeText.text = "";
        }

        /// <summary>Kiểm tra slot này có dữ liệu hay không.</summary>
        public bool HasData()
        {
            return SaveManager.Instance != null && SaveManager.Instance.DoesSlotExist(slotIndex);
        }

        /// <summary>Bật/tắt nút bấm của slot này.</summary>
        public void SetInteractable(bool interactable)
        {
            if (slotButton != null)
            {
                slotButton.interactable = interactable;
            }
        }

        public int SlotIndex => slotIndex;
    }
}
