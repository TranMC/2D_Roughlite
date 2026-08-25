using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Combat;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// UI Element hiển thị thẻ thông tin và các nút tương tác cho 1 vũ khí trong Cửa Hàng Vũ Khí.
    /// </summary>
    public class WeaponShopItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI requirementText;
        [SerializeField] private TextMeshProUGUI priceText;

        [Header("Buttons & Status Badges")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private GameObject equippedBadge;
        [SerializeField] private GameObject lockedBadge;

        private WeaponData currentWeapon;
        private System.Action<WeaponData> onBuyClicked;
        private System.Action<WeaponData> onEquipClicked;

        public WeaponData CurrentWeapon => currentWeapon;

        public void Setup(WeaponData weapon, System.Action<WeaponData> buyCallback, System.Action<WeaponData> equipCallback)
        {
            currentWeapon = weapon;
            onBuyClicked = buyCallback;
            onEquipClicked = equipCallback;

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(currentWeapon));
            }

            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(() => onEquipClicked?.Invoke(currentWeapon));
            }

            RefreshUI();
        }

        public void RefreshUI()
        {
            if (currentWeapon == null) return;

            // 1. Basic Info
            if (iconImage != null && currentWeapon.Icon != null) iconImage.sprite = currentWeapon.Icon;
            if (weaponNameText != null) weaponNameText.text = currentWeapon.WeaponName;
            if (descriptionText != null) descriptionText.text = currentWeapon.Description;

            // 2. Stats
            if (statsText != null)
            {
                statsText.text = $"Sát thương: <color=#ff4d4d>{currentWeapon.Damage}</color> | Tốc đánh: <color=#00e5ff>{currentWeapon.AttackSpeed}</color> | Tầm: {currentWeapon.Range}";
            }

            // 3. Status checks
            bool isUnlocked = WeaponShopManager.Instance != null && WeaponShopManager.Instance.IsWeaponUnlocked(currentWeapon);
            bool isEquipped = WeaponShopManager.Instance != null && WeaponShopManager.Instance.IsWeaponEquipped(currentWeapon);
            bool isReqMet = WeaponShopManager.Instance != null && WeaponShopManager.Instance.IsRequirementMet(currentWeapon);
            bool canAfford = WeaponShopManager.Instance != null && WeaponShopManager.Instance.CanAffordWeapon(currentWeapon);

            // 4. Requirement text
            if (requirementText != null)
            {
                if (currentWeapon.IsDefaultUnlocked || isUnlocked)
                {
                    requirementText.text = "<color=#00ff88>✓ Đã mở khóa</color>";
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
                        string c = kills >= currentWeapon.RequiredEnemiesKilled ? "#00ff88" : "#ff4d4d";
                        reqs.Add($"Diệt quái: <color={c}>{kills}/{currentWeapon.RequiredEnemiesKilled}</color>");
                    }
                    if (currentWeapon.RequiredRunsPlayed > 0)
                    {
                        string c = runs >= currentWeapon.RequiredRunsPlayed ? "#00ff88" : "#ff4d4d";
                        reqs.Add($"Lượt run: <color={c}>{runs}/{currentWeapon.RequiredRunsPlayed}</color>");
                    }
                    if (currentWeapon.RequiredHighestRoom > 0)
                    {
                        string c = rooms >= currentWeapon.RequiredHighestRoom ? "#00ff88" : "#ff4d4d";
                        reqs.Add($"Cấp phòng: <color={c}>{rooms}/{currentWeapon.RequiredHighestRoom}</color>");
                    }

                    requirementText.text = reqs.Count > 0 ? string.Join(" | ", reqs) : "Chưa đủ điều kiện";
                }
            }

            // 5. Price text
            if (priceText != null)
            {
                string color = canAfford ? "#ffcc00" : "#ff4d4d";
                priceText.text = isUnlocked ? "Đã sở hữu" : $"<color={color}>{currentWeapon.Price} Vàng</color>";
            }

            // 6. Badges and Buttons states
            if (equippedBadge != null) equippedBadge.SetActive(isEquipped);
            if (lockedBadge != null) lockedBadge.SetActive(!isUnlocked && !isReqMet);

            if (buyButton != null)
            {
                buyButton.gameObject.SetActive(!isUnlocked);
                buyButton.interactable = !isUnlocked && isReqMet && canAfford;
            }

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(isUnlocked);

                int equippedCount = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.GetEquippedCount() : 0;
                TextMeshProUGUI btnText = equipButton.GetComponentInChildren<TextMeshProUGUI>();

                if (isEquipped)
                {
                    if (btnText != null) btnText.text = "✓ Gỡ Support";
                    equipButton.interactable = true;
                }
                else
                {
                    if (btnText != null) btnText.text = equippedCount >= WeaponUnlockData.MAX_EQUIPPED_SLOTS ? "Đầy Slot (3/3)" : "+ Trang Bị Support";
                    equipButton.interactable = equippedCount < WeaponUnlockData.MAX_EQUIPPED_SLOTS;
                }
            }
        }
    }
}
