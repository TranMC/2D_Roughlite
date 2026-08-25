using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.UpgradeSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Script điều khiển hiển thị 1 phần tử (Item Row/Card) Nâng cấp Vĩnh viễn trong danh sách Scroll.
    /// </summary>
    public class PermanentUpgradeItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI requirementText;
        [SerializeField] private GameObject milestoneBadge;
        [SerializeField] private GameObject maxLevelBadge;
        [SerializeField] private GameObject lockedBadge;

        private PermanentUpgradeData currentUpgrade;
        private Action<PermanentUpgradeData> onBuyClickedCallback;

        private void Awake()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(HandleBuyClick);
            }
        }

        private void OnDestroy()
        {
            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(HandleBuyClick);
            }
        }

        /// <summary>
        /// Khởi tạo và thiết lập hiển thị cho Item UI.
        /// </summary>
        public void Setup(PermanentUpgradeData upgrade, Action<PermanentUpgradeData> onBuyCallback)
        {
            currentUpgrade = upgrade;
            onBuyClickedCallback = onBuyCallback;
            RefreshUI();
        }

        /// <summary>
        /// Cập nhật lại toàn bộ nội dung hiển thị (Tên, Level, Chi phí, Nút Mua, Badge Milestone).
        /// </summary>
        public void RefreshUI()
        {
            if (currentUpgrade == null || PermanentUpgradeManager.Instance == null) return;

            int currentLevel = PermanentUpgradeManager.Instance.GetUpgradeLevel(currentUpgrade.UpgradeId);
            int maxLevel = currentUpgrade.MaxLevel;
            bool isMaxed = currentLevel >= maxLevel;
            bool isReqMet = PermanentUpgradeManager.Instance.IsRequirementMet(currentUpgrade);

            // Icon & Tên
            if (iconImage != null && currentUpgrade.Icon != null)
            {
                iconImage.sprite = currentUpgrade.Icon;
                iconImage.enabled = true;
            }

            if (nameText != null)
            {
                nameText.text = currentUpgrade.UpgradeName;
            }

            // Level text (ví dụ: "Lv 2/5")
            if (levelText != null)
            {
                levelText.text = isMaxed ? $"<color=#FFD700>LV. MAX ({maxLevel})</color>" : $"LV. {currentLevel}/{maxLevel}";
            }

            // Description text
            if (descriptionText != null)
            {
                descriptionText.text = currentUpgrade.Description;
            }

            // Requirement text
            if (requirementText != null)
            {
                if (currentUpgrade.IsDefaultUnlocked || isReqMet)
                {
                    requirementText.text = "<color=#00ff88>✓ Đã mở bán</color>";
                }
                else
                {
                    var reqs = new System.Collections.Generic.List<string>();
                    var progress = Roguelite.SaveSystem.SaveManager.Instance?.CurrentSaveData?.progressData;
                    int kills = progress != null ? progress.totalEnemiesKilled : 0;
                    int runs = progress != null ? progress.totalRunsPlayed : 0;
                    int rooms = progress != null ? progress.highestRoomReached : 0;

                    if (currentUpgrade.RequiredEnemiesKilled > 0)
                    {
                        string c = kills >= currentUpgrade.RequiredEnemiesKilled ? "#00ff88" : "#ff4d4d";
                        reqs.Add($"Diệt quái: <color={c}>{kills}/{currentUpgrade.RequiredEnemiesKilled}</color>");
                    }
                    if (currentUpgrade.RequiredRunsPlayed > 0)
                    {
                        string c = runs >= currentUpgrade.RequiredRunsPlayed ? "#00ff88" : "#ff4d4d";
                        reqs.Add($"Runs: <color={c}>{runs}/{currentUpgrade.RequiredRunsPlayed}</color>");
                    }
                    if (currentUpgrade.RequiredHighestRoom > 0)
                    {
                        string c = rooms >= currentUpgrade.RequiredHighestRoom ? "#00ff88" : "#ff4d4d";
                        reqs.Add($"Rooms: <color={c}>{rooms}/{currentUpgrade.RequiredHighestRoom}</color>");
                    }

                    requirementText.text = reqs.Count > 0 ? string.Join(" | ", reqs) : "Chưa mở bán";
                }
            }

            // Milestone Badge check
            PermanentUpgradeTier nextTier = currentUpgrade.GetTier(currentLevel + 1);
            if (milestoneBadge != null)
            {
                milestoneBadge.SetActive(!isMaxed && nextTier != null && nextTier.isMilestone);
            }

            // Max Level Badge check
            if (maxLevelBadge != null)
            {
                maxLevelBadge.SetActive(isMaxed);
            }

            // Locked Badge check
            if (lockedBadge != null)
            {
                lockedBadge.SetActive(!isReqMet);
            }

            // Cost & Buy button state
            if (isMaxed)
            {
                if (costText != null) costText.text = "MAXED";
                if (buyButton != null) buyButton.interactable = false;
            }
            else
            {
                int cost = currentUpgrade.GetCostForNextLevel(currentLevel);
                if (costText != null)
                {
                    costText.text = !isReqMet ? "<color=#ff4d4d>Chưa đủ ĐK</color>" : $"{cost} Gold";
                }

                bool canAfford = PermanentUpgradeManager.Instance.CanAffordUpgrade(currentUpgrade);
                if (buyButton != null) buyButton.interactable = isReqMet && canAfford;
            }
        }

        private void HandleBuyClick()
        {
            if (currentUpgrade != null)
            {
                onBuyClickedCallback?.Invoke(currentUpgrade);
            }
        }
    }
}
