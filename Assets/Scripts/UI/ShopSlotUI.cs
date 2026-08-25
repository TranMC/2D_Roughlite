using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Combat;
using Roguelite.UpgradeSystem;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    public enum ShopSlotType
    {
        Weapon,
        PermanentUpgrade
    }

    /// <summary>
    /// Component điều khiển hiển thị 1 ô/dòng sản phẩm trong Cửa Hàng (Shop Slot UI).
    /// Hỗ trợ hiển thị cả Vũ Khí (Weapon) và Nâng Cấp Vĩnh Viễn (Permanent Upgrade).
    /// Hỗ trợ linh hoạt cả giao diện 1 nút (tự đổi Mua -> Trang Bị/Gỡ Bỏ) hoặc 2 nút riêng biệt.
    /// Version: 1.3.0
    /// </summary>
    public class ShopSlotUI : MonoBehaviour
    {
        public const string VERSION = "1.3.0";

        [Header("--- UI References ---")]
        [Tooltip("Ảnh hiển thị icon vũ khí hoặc nâng cấp.")]
        [SerializeField] private Image iconImage;

        [Tooltip("Tên vũ khí hoặc tên nâng cấp.")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("Thông số (Damage, Speed) hoặc Cấp độ (Lv 1/5).")]
        [SerializeField] private TextMeshProUGUI subTitleText;

        [Tooltip("Mô tả chi tiết công dụng/hiệu ứng.")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Tooltip("Điều kiện mở khóa (Diệt quái, Vượt phòng...).")]
        [SerializeField] private TextMeshProUGUI requirementText;

        [Tooltip("Giá tiền vàng.")]
        [SerializeField] private TextMeshProUGUI priceText;

        [Header("--- Buttons & Badges ---")]
        [Tooltip("Nút chính (Mua / Nâng Cấp / Trang Bị thích ứng).")]
        [SerializeField] private Button mainActionButton;
        [SerializeField] private TextMeshProUGUI mainActionText;

        [Tooltip("Nút phụ Trang bị / Gỡ bỏ (Tùy chọn nếu muốn tách riêng nút).")]
        [SerializeField] private Button equipButton;
        [SerializeField] private TextMeshProUGUI equipActionText;

        [Tooltip("Badge hiển thị khi đã trang bị.")]
        [SerializeField] private GameObject equippedBadge;

        [Tooltip("Badge/Overlay khi chưa đạt điều kiện mở khóa.")]
        [SerializeField] private GameObject lockedBadge;

        [Tooltip("Badge khi đã nâng cấp tối đa (Max Level).")]
        [SerializeField] private GameObject maxLevelBadge;

        private ShopSlotType currentType;
        private WeaponData currentWeapon;
        private PermanentUpgradeData currentUpgrade;

        private Action<WeaponData> onWeaponBuyCallback;
        private Action<WeaponData> onWeaponEquipCallback;
        private Action<PermanentUpgradeData> onUpgradeBuyCallback;

        private void Awake()
        {
            if (mainActionButton != null)
            {
                mainActionButton.onClick.AddListener(HandleMainActionClick);
            }
            if (equipButton != null)
            {
                equipButton.onClick.AddListener(HandleEquipClick);
            }
        }

        private void OnDestroy()
        {
            if (mainActionButton != null)
            {
                mainActionButton.onClick.RemoveListener(HandleMainActionClick);
            }
            if (equipButton != null)
            {
                equipButton.onClick.RemoveListener(HandleEquipClick);
            }
        }

        /// <summary>
        /// Cấu hình hiển thị ô cho Vũ Khí (Equipment).
        /// </summary>
        public void SetupWeapon(WeaponData weapon, Action<WeaponData> buyCallback, Action<WeaponData> equipCallback)
        {
            currentType = ShopSlotType.Weapon;
            currentWeapon = weapon;
            currentUpgrade = null;
            onWeaponBuyCallback = buyCallback;
            onWeaponEquipCallback = equipCallback;

            RefreshUI();
        }

        /// <summary>
        /// Cấu hình hiển thị ô cho Nâng Cấp Vĩnh Viễn (Power/Upgrade).
        /// </summary>
        public void SetupUpgrade(PermanentUpgradeData upgrade, Action<PermanentUpgradeData> buyCallback)
        {
            currentType = ShopSlotType.PermanentUpgrade;
            currentUpgrade = upgrade;
            currentWeapon = null;
            onUpgradeBuyCallback = buyCallback;

            RefreshUI();
        }

        /// <summary>
        /// Làm mới nội dung hiển thị của Slot dựa trên loại dữ liệu hiện tại.
        /// </summary>
        public void RefreshUI()
        {
            if (currentType == ShopSlotType.Weapon)
            {
                RefreshWeaponUI();
            }
            else
            {
                RefreshUpgradeUI();
            }
        }

        private void RefreshWeaponUI()
        {
            if (currentWeapon == null) return;

            // 1. Icon & Tên
            if (iconImage != null)
            {
                iconImage.sprite = currentWeapon.Icon;
                iconImage.enabled = currentWeapon.Icon != null;
            }
            if (titleText != null) titleText.text = currentWeapon.WeaponName;

            // 2. Chỉ số & Mô tả
            if (subTitleText != null)
            {
                subTitleText.text = $"DMG: <color=#ff5555>{currentWeapon.Damage}</color> | SPD: <color=#00e5ff>{currentWeapon.AttackSpeed}s</color> | RNG: <color=#ffd700>{currentWeapon.Range}</color>";
            }
            if (descriptionText != null) descriptionText.text = currentWeapon.Description;

            // 3. Trạng thái sở hữu & trang bị
            bool isUnlocked = WeaponShopManager.Instance != null
                ? WeaponShopManager.Instance.IsWeaponUnlocked(currentWeapon)
                : currentWeapon.IsDefaultUnlocked;

            bool isEquipped = WeaponShopManager.Instance != null && WeaponShopManager.Instance.IsWeaponEquipped(currentWeapon);
            bool isReqMet = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.IsRequirementMet(currentWeapon) : currentWeapon.IsDefaultUnlocked;
            bool canAfford = WeaponShopManager.Instance != null && WeaponShopManager.Instance.CanAffordWeapon(currentWeapon);
            int equippedCount = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.GetEquippedCount() : 0;
            bool canEquip = equippedCount < WeaponUnlockData.MAX_EQUIPPED_SLOTS;

            // 4. Điều kiện mở khóa
            if (requirementText != null)
            {
                if (currentWeapon.IsDefaultUnlocked || isUnlocked)
                {
                    requirementText.text = "<color=#55ff55>Đã mở khoá</color>";
                }
                else
                {
                    var reqs = new System.Collections.Generic.List<string>();
                    var progress = SaveManager.Instance?.CurrentSaveData?.progressData;
                    int kills = progress != null ? progress.totalEnemiesKilled : 0;
                    int runs = progress != null ? progress.totalRunsPlayed : 0;
                    int rooms = progress != null ? progress.highestRoomReached : 0;

                    if (currentWeapon.RequiredEnemiesKilled > 0)
                    {
                        string c = kills >= currentWeapon.RequiredEnemiesKilled ? "#55ff55" : "#ff5555";
                        reqs.Add($"Diệt quái: <color={c}>{kills}/{currentWeapon.RequiredEnemiesKilled}</color>");
                    }
                    if (currentWeapon.RequiredRunsPlayed > 0)
                    {
                        string c = runs >= currentWeapon.RequiredRunsPlayed ? "#55ff55" : "#ff5555";
                        reqs.Add($"Số run: <color={c}>{runs}/{currentWeapon.RequiredRunsPlayed}</color>");
                    }
                    if (currentWeapon.RequiredHighestRoom > 0)
                    {
                        string c = rooms >= currentWeapon.RequiredHighestRoom ? "#55ff55" : "#ff5555";
                        reqs.Add($"Cấp phòng: <color={c}>{rooms}/{currentWeapon.RequiredHighestRoom}</color>");
                    }

                    requirementText.text = reqs.Count > 0 ? string.Join(" | ", reqs) : "Chưa đủ điều kiện";
                }
            }

            // 5. Giá tiền
            if (priceText != null)
            {
                if (isUnlocked)
                {
                    priceText.text = isEquipped ? "<color=#00e5ff>Đang trang bị</color>" : "<color=#88ff88>Đã sở hữu</color>";
                }
                else
                {
                    string priceColor = canAfford ? "#ffd700" : "#ff5555";
                    priceText.text = $"<color={priceColor}>{currentWeapon.Price} Vàng</color>";
                }
            }

            // 6. Badges
            if (equippedBadge != null) equippedBadge.SetActive(isEquipped);
            if (lockedBadge != null) lockedBadge.SetActive(!isUnlocked && !isReqMet);
            if (maxLevelBadge != null) maxLevelBadge.SetActive(false);

            // 7. Buttons (Hỗ trợ cả 1 nút duy nhất và 2 nút riêng biệt)
            if (equipButton != null)
            {
                // Chế độ 2 nút riêng biệt
                if (mainActionButton != null)
                {
                    mainActionButton.gameObject.SetActive(!isUnlocked);
                    mainActionButton.interactable = !isUnlocked && isReqMet && canAfford;
                    if (mainActionText != null) mainActionText.text = "Mua";
                }

                equipButton.gameObject.SetActive(isUnlocked);
                if (isEquipped)
                {
                    if (equipActionText != null) equipActionText.text = "Gỡ Bỏ";
                    equipButton.interactable = true;
                }
                else
                {
                    if (equipActionText != null) equipActionText.text = canEquip ? "Trang Bị" : "Đầy Slot (3/3)";
                    equipButton.interactable = canEquip;
                }
            }
            else if (mainActionButton != null)
            {
                // Chế độ 1 nút tự thích ứng
                mainActionButton.gameObject.SetActive(true);

                if (!isUnlocked)
                {
                    mainActionButton.interactable = isReqMet && canAfford;
                    if (mainActionText != null) mainActionText.text = "Mua";
                }
                else
                {
                    if (isEquipped)
                    {
                        mainActionButton.interactable = true;
                        if (mainActionText != null) mainActionText.text = "Gỡ Bỏ";
                    }
                    else
                    {
                        mainActionButton.interactable = canEquip;
                        if (mainActionText != null) mainActionText.text = canEquip ? "Trang Bị" : "Đầy (3/3)";
                    }
                }
            }
        }

        private void RefreshUpgradeUI()
        {
            if (currentUpgrade == null) return;

            int currentLevel = PermanentUpgradeManager.Instance != null
                ? PermanentUpgradeManager.Instance.GetUpgradeLevel(currentUpgrade.UpgradeId)
                : (SaveManager.Instance?.CurrentSaveData?.abilityData?.GetAbilityLevel(currentUpgrade.UpgradeId) ?? 0);

            int maxLevel = currentUpgrade.MaxLevel;
            bool isMaxed = currentLevel >= maxLevel;
            bool isReqMet = PermanentUpgradeManager.Instance != null
                ? PermanentUpgradeManager.Instance.IsRequirementMet(currentUpgrade)
                : currentUpgrade.IsDefaultUnlocked;

            bool canAfford = PermanentUpgradeManager.Instance != null
                ? PermanentUpgradeManager.Instance.CanAffordUpgrade(currentUpgrade)
                : false;

            // 1. Icon & Tên
            if (iconImage != null)
            {
                iconImage.sprite = currentUpgrade.Icon;
                iconImage.enabled = currentUpgrade.Icon != null;
            }
            if (titleText != null) titleText.text = currentUpgrade.UpgradeName;

            // 2. Cấp độ & Mô tả
            if (subTitleText != null)
            {
                subTitleText.text = isMaxed ? "<color=#ffd700>LV. MAX</color>" : $"Cấp Độ: <color=#00e5ff>{currentLevel}/{maxLevel}</color>";
            }
            if (descriptionText != null) descriptionText.text = currentUpgrade.Description;

            // 3. Điều kiện mở khóa
            if (requirementText != null)
            {
                if (currentUpgrade.IsDefaultUnlocked || isReqMet)
                {
                    requirementText.text = "<color=#55ff55>Đã mở bán</color>";
                }
                else
                {
                    var reqs = new System.Collections.Generic.List<string>();
                    var progress = SaveManager.Instance?.CurrentSaveData?.progressData;
                    int kills = progress != null ? progress.totalEnemiesKilled : 0;
                    int runs = progress != null ? progress.totalRunsPlayed : 0;
                    int rooms = progress != null ? progress.highestRoomReached : 0;

                    if (currentUpgrade.RequiredEnemiesKilled > 0)
                    {
                        string c = kills >= currentUpgrade.RequiredEnemiesKilled ? "#55ff55" : "#ff5555";
                        reqs.Add($"Diệt quái: <color={c}>{kills}/{currentUpgrade.RequiredEnemiesKilled}</color>");
                    }
                    if (currentUpgrade.RequiredRunsPlayed > 0)
                    {
                        string c = runs >= currentUpgrade.RequiredRunsPlayed ? "#55ff55" : "#ff5555";
                        reqs.Add($"Số run: <color={c}>{runs}/{currentUpgrade.RequiredRunsPlayed}</color>");
                    }
                    if (currentUpgrade.RequiredHighestRoom > 0)
                    {
                        string c = rooms >= currentUpgrade.RequiredHighestRoom ? "#55ff55" : "#ff5555";
                        reqs.Add($"Cấp phòng: <color={c}>{rooms}/{currentUpgrade.RequiredHighestRoom}</color>");
                    }

                    requirementText.text = reqs.Count > 0 ? string.Join(" | ", reqs) : "Chưa đủ điều kiện";
                }
            }

            // 4. Giá tiền
            int nextCost = currentUpgrade.GetCostForNextLevel(currentLevel);

            if (priceText != null)
            {
                if (isMaxed)
                {
                    priceText.text = "<color=#ffd700>TỐI ĐA</color>";
                }
                else if (nextCost > 0)
                {
                    string color = canAfford ? "#ffd700" : "#ff5555";
                    priceText.text = $"<color={color}>{nextCost} Vàng</color>";
                }
                else
                {
                    priceText.text = "<color=#ff5555>Chưa mở</color>";
                }
            }

            // 5. Badges
            if (equippedBadge != null) equippedBadge.SetActive(false);
            if (lockedBadge != null) lockedBadge.SetActive(!isReqMet);
            if (maxLevelBadge != null) maxLevelBadge.SetActive(isMaxed);

            // 6. Action Button
            if (mainActionButton != null)
            {
                mainActionButton.gameObject.SetActive(true);
                mainActionButton.interactable = !isMaxed && isReqMet && canAfford;
                if (mainActionText != null)
                {
                    mainActionText.text = isMaxed ? "Đã Max" : (currentLevel == 0 ? "Mua" : "Nâng Cấp");
                }
            }

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(false);
            }
        }

        private void HandleMainActionClick()
        {
            if (currentType == ShopSlotType.Weapon && currentWeapon != null)
            {
                bool isUnlocked = WeaponShopManager.Instance != null
                    ? WeaponShopManager.Instance.IsWeaponUnlocked(currentWeapon)
                    : currentWeapon.IsDefaultUnlocked;

                // Nếu là 1 nút duy nhất và vũ khí đã sở hữu -> thực hiện Trang bị / Gỡ bỏ
                if (equipButton == null && isUnlocked)
                {
                    onWeaponEquipCallback?.Invoke(currentWeapon);
                }
                else
                {
                    onWeaponBuyCallback?.Invoke(currentWeapon);
                }
            }
            else if (currentType == ShopSlotType.PermanentUpgrade && currentUpgrade != null)
            {
                onUpgradeBuyCallback?.Invoke(currentUpgrade);
            }
        }

        private void HandleEquipClick()
        {
            if (currentType == ShopSlotType.Weapon && currentWeapon != null)
            {
                onWeaponEquipCallback?.Invoke(currentWeapon);
            }
        }
    }
}
