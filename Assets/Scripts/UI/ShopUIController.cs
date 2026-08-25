using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Combat;
using Roguelite.UpgradeSystem;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    public enum ShopTab
    {
        Equipment, // Vũ khí & Trang bị
        Power      // Nâng cấp chỉ số vĩnh viễn
    }

    /// <summary>
    /// Controller chính điều khiển toàn bộ Giao diện Shop (Shop Canvas / Shop Panel).
    /// Quản lý chuyển Tab (Equipment vs Power), đồng bộ hiển thị Vàng và danh sách mặt hàng.
    /// Version: 1.0.0
    /// </summary>
    public class ShopUIController : MonoBehaviour
    {
        public const string VERSION = "1.1.0";
        public static ShopUIController Instance { get; private set; }

        [Header("--- Shop Panel Root ---")]
        [Tooltip("Panel chính của cửa hàng.")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeButton;

        [Header("--- Header & Currency ---")]
        [Tooltip("Text hiển thị tổng số tiền vàng của người chơi.")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("--- Tab Navigation Buttons ---")]
        [Tooltip("Nút chuyển sang tab Vũ Khí (Equipment).")]
        [SerializeField] private Button equipmentTabButton;

        [Tooltip("Nút chuyển sang tab Nâng cấp vĩnh viễn (Power / Stats).")]
        [SerializeField] private Button powerTabButton;

        [Header("--- Tab Visual Styling ---")]
        [SerializeField] private Color activeTabColor = new Color(1f, 0.85f, 0.4f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

        [Header("--- Scroll View & Slot Container ---")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private ShopSlotUI slotPrefab;
        [SerializeField] private GameObject emptyStateText;

        [Header("--- Optional Category Filter (Power Tab) ---")]
        [SerializeField] private GameObject categoryFilterGroup;
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterOffenseButton;
        [SerializeField] private Button filterDefenseButton;
        [SerializeField] private Button filterUtilityButton;

        private ShopTab currentTab = ShopTab.Equipment;
        private PermanentUpgradeCategory currentUpgradeCategory = PermanentUpgradeCategory.All;
        private readonly List<ShopSlotUI> spawnedSlots = new List<ShopSlotUI>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupButtons();
        }

        private void OnEnable()
        {
            // Lắng nghe sự kiện từ WeaponShopManager
            WeaponShopManager.OnWeaponUnlockStateChanged += HandleShopDataChanged;
            WeaponShopManager.OnWeaponPurchased += HandleWeaponPurchased;
            WeaponShopManager.OnWeaponEquipped += HandleWeaponEquipped;

            // Lắng nghe sự kiện từ PermanentUpgradeManager & SaveManager
            PermanentUpgradeManager.OnUpgradePurchased += HandleUpgradePurchased;
            PermanentUpgradeManager.OnCurrencyChanged += HandleCurrencyChanged;
            SaveManager.OnSaveCompleted += HandleShopDataChanged;

            SwitchTab(ShopTab.Equipment);
            UpdateGoldDisplay();
        }

        private void OnDisable()
        {
            WeaponShopManager.OnWeaponUnlockStateChanged -= HandleShopDataChanged;
            WeaponShopManager.OnWeaponPurchased -= HandleWeaponPurchased;
            WeaponShopManager.OnWeaponEquipped -= HandleWeaponEquipped;

            PermanentUpgradeManager.OnUpgradePurchased -= HandleUpgradePurchased;
            PermanentUpgradeManager.OnCurrencyChanged -= HandleCurrencyChanged;
            SaveManager.OnSaveCompleted -= HandleShopDataChanged;
        }

        private void SetupButtons()
        {
            if (equipmentTabButton != null)
            {
                equipmentTabButton.onClick.RemoveAllListeners();
                equipmentTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Equipment));
            }

            if (powerTabButton != null)
            {
                powerTabButton.onClick.RemoveAllListeners();
                powerTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Power));
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseShop);
            }

            // Setup Filter nút cho Power Tab nếu có
            if (filterAllButton != null)
            {
                filterAllButton.onClick.RemoveAllListeners();
                filterAllButton.onClick.AddListener(() => SelectUpgradeCategory(PermanentUpgradeCategory.All));
            }
            if (filterOffenseButton != null)
            {
                filterOffenseButton.onClick.RemoveAllListeners();
                filterOffenseButton.onClick.AddListener(() => SelectUpgradeCategory(PermanentUpgradeCategory.Offense));
            }
            if (filterDefenseButton != null)
            {
                filterDefenseButton.onClick.RemoveAllListeners();
                filterDefenseButton.onClick.AddListener(() => SelectUpgradeCategory(PermanentUpgradeCategory.Defense));
            }
            if (filterUtilityButton != null)
            {
                filterUtilityButton.onClick.RemoveAllListeners();
                filterUtilityButton.onClick.AddListener(() => SelectUpgradeCategory(PermanentUpgradeCategory.Utility));
            }
        }

        /// <summary>
        /// Chuyển đổi qua lại giữa Tab Equipment và Tab Power.
        /// </summary>
        public void SwitchTab(ShopTab tab)
        {
            currentTab = tab;
            UpdateTabButtonVisuals();
            PopulateCurrentTabItems();

            if (categoryFilterGroup != null)
            {
                categoryFilterGroup.SetActive(tab == ShopTab.Power);
            }

            // Reset vị trí cuộn về đầu
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Lọc danh mục cho Tab Power (All / Công / Thủ / Tiện ích).
        /// </summary>
        public void SelectUpgradeCategory(PermanentUpgradeCategory category)
        {
            currentUpgradeCategory = category;
            if (currentTab == ShopTab.Power)
            {
                PopulateCurrentTabItems();
            }
        }

        /// <summary>
        /// Sinh các item vào Content container theo Tab hiện tại.
        /// </summary>
        public void PopulateCurrentTabItems()
        {
            ClearSpawnedSlots();

            if (contentContainer == null || slotPrefab == null)
            {
                Debug.LogWarning("[ShopUIController] Thiếu Content Container hoặc Slot Prefab!");
                return;
            }

            if (currentTab == ShopTab.Equipment)
            {
                PopulateWeapons();
            }
            else
            {
                PopulatePermanentUpgrades();
            }
        }

        private void PopulateWeapons()
        {
            WeaponDatabase db = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.Database : WeaponShopManager.GetOrLoadWeaponDatabase();
            if (db == null || db.AllWeapons == null || db.AllWeapons.Count == 0)
            {
                if (emptyStateText != null) emptyStateText.SetActive(true);
                return;
            }

            if (emptyStateText != null) emptyStateText.SetActive(false);

            foreach (WeaponData weapon in db.AllWeapons)
            {
                if (weapon == null) continue;

                ShopSlotUI slot = Instantiate(slotPrefab, contentContainer);
                slot.SetupWeapon(weapon, OnWeaponBuyClicked, OnWeaponEquipClicked);
                spawnedSlots.Add(slot);
            }
        }

        private void PopulatePermanentUpgrades()
        {
            var upgradeDb = PermanentUpgradeManager.Instance != null ? PermanentUpgradeManager.Instance.Database : PermanentUpgradeManager.GetOrLoadPermanentUpgradeDatabase();
            if (upgradeDb == null)
            {
                if (emptyStateText != null) emptyStateText.SetActive(true);
                return;
            }

            List<PermanentUpgradeData> upgrades = upgradeDb.GetUpgradesByCategory(currentUpgradeCategory);
            if (upgrades == null || upgrades.Count == 0)
            {
                if (emptyStateText != null) emptyStateText.SetActive(true);
                return;
            }

            if (emptyStateText != null) emptyStateText.SetActive(false);

            foreach (PermanentUpgradeData upgrade in upgrades)
            {
                if (upgrade == null) continue;

                ShopSlotUI slot = Instantiate(slotPrefab, contentContainer);
                slot.SetupUpgrade(upgrade, OnUpgradeBuyClicked);
                spawnedSlots.Add(slot);
            }
        }

        private void OnWeaponBuyClicked(WeaponData weapon)
        {
            if (WeaponShopManager.Instance == null || weapon == null) return;

            bool success = WeaponShopManager.Instance.TryPurchaseWeapon(weapon);
            if (success)
            {
                RefreshAllSpawnedSlots();
                UpdateGoldDisplay();
                PlayPerkSelectSound();
            }
        }

        private void OnWeaponEquipClicked(WeaponData weapon)
        {
            if (WeaponShopManager.Instance == null || weapon == null) return;

            bool success = WeaponShopManager.Instance.ToggleEquipSupportWeapon(weapon);
            if (success)
            {
                RefreshAllSpawnedSlots();
                PlayPerkSelectSound();
            }
        }

        private void OnUpgradeBuyClicked(PermanentUpgradeData upgrade)
        {
            if (PermanentUpgradeManager.Instance == null || upgrade == null) return;

            bool success = PermanentUpgradeManager.Instance.TryPurchaseUpgrade(upgrade);
            if (success)
            {
                RefreshAllSpawnedSlots();
                UpdateGoldDisplay();
                PlayPerkSelectSound();
            }
        }

        private void RefreshAllSpawnedSlots()
        {
            foreach (var slot in spawnedSlots)
            {
                if (slot != null)
                {
                    slot.RefreshUI();
                }
            }
        }

        private void UpdateTabButtonVisuals()
        {
            if (equipmentTabButton != null)
            {
                var img = equipmentTabButton.GetComponent<Image>();
                if (img != null) img.color = currentTab == ShopTab.Equipment ? activeTabColor : inactiveTabColor;
            }

            if (powerTabButton != null)
            {
                var img = powerTabButton.GetComponent<Image>();
                if (img != null) img.color = currentTab == ShopTab.Power ? activeTabColor : inactiveTabColor;
            }
        }

        public void UpdateGoldDisplay()
        {
            if (goldText != null)
            {
                int gold = SaveManager.Instance?.CurrentSaveData?.progressData?.totalCurrency ?? 0;
                goldText.text = $"<color=#ffd700>{gold:N0}</color> Vàng";
            }
        }

        public void OpenShop()
        {
            if (shopPanel != null) shopPanel.SetActive(true);
            SwitchTab(currentTab);
            UpdateGoldDisplay();
        }

        public void CloseShop()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
        }

        public void ToggleShop()
        {
            if (shopPanel != null && shopPanel.activeSelf)
            {
                CloseShop();
            }
            else
            {
                OpenShop();
            }
        }

        private void ClearSpawnedSlots()
        {
            foreach (var slot in spawnedSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            spawnedSlots.Clear();
        }

        private void HandleShopDataChanged()
        {
            UpdateGoldDisplay();
            RefreshAllSpawnedSlots();
        }

        private void HandleWeaponPurchased(WeaponData weapon) => HandleShopDataChanged();
        private void HandleWeaponEquipped(WeaponData weapon) => HandleShopDataChanged();
        private void HandleUpgradePurchased(PermanentUpgradeData data, int level) => HandleShopDataChanged();
        private void HandleCurrencyChanged(int amount) => UpdateGoldDisplay();

        private void PlayPerkSelectSound()
        {
            SoundManager.PlaySound(SoundType.PERK_SELECT);
        }
    }
}
