using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.UpgradeSystem;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Controller chính điều khiển UI Cửa hàng Nâng cấp Vĩnh viễn (Permanent Upgrade Shop UI).
    /// Hỗ trợ lọc theo danh mục (Filter), cuộn danh sách (Scroll Rect), và tự động cập nhật khi mua nâng cấp/thay đổi tiền.
    /// </summary>
    public class PermanentUpgradeUIController : MonoBehaviour
    {
        [Header("UI Header References")]
        [SerializeField] private TextMeshProUGUI totalCurrencyText;

        [Header("Filter Tab Buttons")]
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterOffenseButton;
        [SerializeField] private Button filterDefenseButton;
        [SerializeField] private Button filterUtilityButton;

        [Header("Scroll Container References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private PermanentUpgradeItemUI itemPrefab;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStateText;

        private PermanentUpgradeCategory currentCategory = PermanentUpgradeCategory.All;
        private List<PermanentUpgradeItemUI> spawnedItems = new List<PermanentUpgradeItemUI>();

        private void OnEnable()
        {
            // Đăng ký các sự kiện lắng nghe từ PermanentUpgradeManager và SaveManager
            PermanentUpgradeManager.OnUpgradePurchased += HandleUpgradePurchased;
            PermanentUpgradeManager.OnCurrencyChanged += UpdateCurrencyDisplay;

            SetupFilterButtons();
            RefreshAll();
        }

        private void OnDisable()
        {
            PermanentUpgradeManager.OnUpgradePurchased -= HandleUpgradePurchased;
            PermanentUpgradeManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
        }

        private void SetupFilterButtons()
        {
            if (filterAllButton != null)
            {
                filterAllButton.onClick.RemoveAllListeners();
                filterAllButton.onClick.AddListener(() => SelectCategory(PermanentUpgradeCategory.All));
            }
            if (filterOffenseButton != null)
            {
                filterOffenseButton.onClick.RemoveAllListeners();
                filterOffenseButton.onClick.AddListener(() => SelectCategory(PermanentUpgradeCategory.Offense));
            }
            if (filterDefenseButton != null)
            {
                filterDefenseButton.onClick.RemoveAllListeners();
                filterDefenseButton.onClick.AddListener(() => SelectCategory(PermanentUpgradeCategory.Defense));
            }
            if (filterUtilityButton != null)
            {
                filterUtilityButton.onClick.RemoveAllListeners();
                filterUtilityButton.onClick.AddListener(() => SelectCategory(PermanentUpgradeCategory.Utility));
            }
        }

        /// <summary>
        /// Làm mới toàn bộ UI: Hiển thị tiền tệ + Tải danh sách nâng cấp theo Filter hiện tại.
        /// </summary>
        public void RefreshAll()
        {
            UpdateCurrencyDisplay(GetTotalCurrency());
            PopulateUpgradeList();
        }

        /// <summary>
        /// Chuyển bộ lọc danh mục (Filter Category) và tải lại danh sách Scroll.
        /// </summary>
        public void SelectCategory(PermanentUpgradeCategory category)
        {
            currentCategory = category;
            Debug.Log($"[PermanentUpgradeUIController] Đã chọn Filter Category: {category}");
            PopulateUpgradeList();

            // Reset vị trí cuộn thanh Scroll về đỉnh
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
        }

        /// <summary>
        /// Đọc và hiển thị danh sách các Upgrade thỏa mãn bộ lọc Filter vào Scroll View.
        /// </summary>
        private void PopulateUpgradeList()
        {
            ClearItems();

            if (PermanentUpgradeManager.Instance == null || PermanentUpgradeManager.Instance.Database == null)
            {
                Debug.LogWarning("[PermanentUpgradeUIController] Chưa có PermanentUpgradeManager hoặc Database!");
                if (emptyStateText != null) emptyStateText.SetActive(true);
                return;
            }

            // Lấy danh sách Upgrade theo Category
            List<PermanentUpgradeData> upgrades = PermanentUpgradeManager.Instance.Database.GetUpgradesByCategory(currentCategory);

            if (upgrades == null || upgrades.Count == 0)
            {
                if (emptyStateText != null) emptyStateText.SetActive(true);
                return;
            }

            if (emptyStateText != null) emptyStateText.SetActive(false);

            // Sinh các Item UI tương ứng
            foreach (var upgrade in upgrades)
            {
                if (upgrade == null) continue;

                PermanentUpgradeItemUI item = Instantiate(itemPrefab, itemContainer);
                item.Setup(upgrade, OnItemBuyClicked);
                spawnedItems.Add(item);
            }
        }

        private void OnItemBuyClicked(PermanentUpgradeData upgradeData)
        {
            if (PermanentUpgradeManager.Instance != null && upgradeData != null)
            {
                bool success = PermanentUpgradeManager.Instance.TryPurchaseUpgrade(upgradeData);
                if (success)
                {
                    RefreshAllItemsUI();
                }
            }
        }

        private void RefreshAllItemsUI()
        {
            foreach (var item in spawnedItems)
            {
                if (item != null)
                {
                    item.RefreshUI();
                }
            }
        }

        private void HandleUpgradePurchased(PermanentUpgradeData upgrade, int newLevel)
        {
            UpdateCurrencyDisplay(GetTotalCurrency());
            RefreshAllItemsUI();
        }

        private void UpdateCurrencyDisplay(int currency)
        {
            if (totalCurrencyText != null)
            {
                totalCurrencyText.text = $"Gold: {currency}";
            }
        }

        private int GetTotalCurrency()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                return SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
            }
            return 0;
        }

        private void ClearItems()
        {
            foreach (var item in spawnedItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            spawnedItems.Clear();
        }
    }
}
