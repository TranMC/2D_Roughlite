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

        private System.Action<int> onSlotSelected;

        /// <summary>Slot này có phải Auto Save không.</summary>
        public bool IsAutoSaveSlot => slotIndex == SaveManager.AUTOSAVE_SLOT_INDEX;

        /// <summary>Gán callback khi người chơi bấm chọn slot này.</summary>
        public void Setup(System.Action<int> callback)
        {
            onSlotSelected = callback;
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onSlotSelected?.Invoke(slotIndex));
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
            slotTitleText.text = IsAutoSaveSlot ? "Auto Save" : $"Slot {slotIndex}";

            string timeInfo = string.IsNullOrEmpty(previewData.lastSavedTime)
                ? "Chưa rõ"
                : previewData.lastSavedTime;

            int runs = previewData.progressData != null ? previewData.progressData.totalRunsPlayed : 0;
            int room = previewData.progressData != null ? previewData.progressData.highestRoomReached : 0;
            int gold = previewData.progressData != null ? previewData.progressData.totalCurrency : 0;

            slotInfoText.text = $"Run: {runs} | Phòng: {room} | Vàng: {gold}\nLưu lúc: {timeInfo}";
        }

        /// <summary>Hiển thị trạng thái trống khi slot chưa có dữ liệu.</summary>
        private void SetEmptyDisplay()
        {
            slotTitleText.text = IsAutoSaveSlot ? "Auto Save" : $"Slot {slotIndex}";
            slotInfoText.text = "[TRỐNG]";
        }

        /// <summary>Kiểm tra slot này có dữ liệu hay không.</summary>
        public bool HasData()
        {
            return SaveManager.Instance != null && SaveManager.Instance.DoesSlotExist(slotIndex);
        }

        /// <summary>Bật/tắt nút bấm của slot này.</summary>
        public void SetInteractable(bool interactable)
        {
            slotButton.interactable = interactable;
        }

        public int SlotIndex => slotIndex;
    }
}
