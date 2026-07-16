using System.Collections.Generic;
using UnityEngine;
using Roguelite.UpgradeSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Điều khiển bảng giao diện chọn Perk phần thưởng (Reward Selection Panel).
    /// Quản lý bật/tắt UI, random các Perk, và bắt sự kiện phím tắt 1-2-3 / click để chọn Perk.
    /// </summary>
    public class RewardSelectionController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Container Panel chính chứa UI chọn Perk để bật/tắt.")]
        [SerializeField] private GameObject selectionPanel;

        [Tooltip("Mảng chứa 3 RewardCardUI để hiển thị 3 lựa chọn.")]
        [SerializeField] private RewardCardUI[] rewardCards;

        // Lưu trữ danh sách Perk đang hiển thị hiện tại
        private List<PerkData> currentOptions = new List<PerkData>();
        private bool isPanelActive = false;

        private void Start()
        {
            // Mặc định ẩn giao diện khi bắt đầu
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
            isPanelActive = false;
        }

        private void Update()
        {
            if (!isPanelActive) return;

            // Bắt phím tắt 1, 2, 3 để chọn nhanh Perk
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ChoosePerk(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ChoosePerk(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ChoosePerk(2);
            }
        }

        /// <summary>
        /// Mở panel chọn Perk và tự động rút 3 lựa chọn ngẫu nhiên từ UpgradeManager.
        /// </summary>
        [ContextMenu("Debug/Open Selection Panel")]
        public void OpenSelection()
        {
            if (selectionPanel == null || rewardCards == null || rewardCards.Length < 3)
            {
                Debug.LogError("[RewardSelectionController] Cấu hình UI chưa đủ hoặc mảng rewardCards không đủ 3 phần tử!");
                return;
            }

            if (UpgradeManager.Instance == null)
            {
                Debug.LogError("[RewardSelectionController] Không tìm thấy UpgradeManager Instance!");
                return;
            }

            // 1. Rút 3 Perk ngẫu nhiên từ pool
            currentOptions = UpgradeManager.Instance.GetRandomPerks(3);

            if (currentOptions.Count == 0)
            {
                Debug.LogWarning("[RewardSelectionController] Không rút được Perk nào do pool cạn!");
                return;
            }

            // 2. Gán dữ liệu cho các card UI
            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (i < currentOptions.Count)
                {
                    rewardCards[i].gameObject.SetActive(true);
                    rewardCards[i].SetupCard(currentOptions[i]);
                }
                else
                {
                    rewardCards[i].gameObject.SetActive(false);
                }
            }

            // 3. Hiển thị Panel UI và dừng thời gian game
            selectionPanel.SetActive(true);
            isPanelActive = true;
            Time.timeScale = 0f;

            Debug.Log("[RewardSelectionController] Đã mở bảng chọn Perk, trò chơi tạm dừng.");
        }

        /// <summary>
        /// Lựa chọn Perk tại index (gọi từ Button Click hoặc Phím tắt).
        /// </summary>
        public void ChoosePerk(int index)
        {
            if (index < 0 || index >= currentOptions.Count) return;

            PerkData chosenPerk = currentOptions[index];
            if (chosenPerk != null)
            {
                // Thêm Perk vào cho Player
                UpgradeManager.Instance.AddPerk(chosenPerk);
                Debug.Log($"[RewardSelectionController] Đã chọn Perk: {chosenPerk.PerkName}");
            }

            // Đóng Panel và khôi phục thời gian
            CloseSelection();
        }

        /// <summary>
        /// Đóng giao diện chọn Perk và tiếp tục game.
        /// </summary>
        private void CloseSelection()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
            isPanelActive = false;
            currentOptions.Clear();
            Time.timeScale = 1f;

            Debug.Log("[RewardSelectionController] Đã đóng bảng chọn Perk, trò chơi tiếp tục.");
        }
    }
}
