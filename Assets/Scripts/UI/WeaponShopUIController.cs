using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Combat;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Controller chính điều khiển UI Cửa Hàng Vũ Khí (Weapon Shop UI Controller).
    /// </summary>
    public class WeaponShopUIController : MonoBehaviour
    {
        public static WeaponShopUIController Instance { get; private set; }

        [Header("UI Panel & Prefabs")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private WeaponShopItemUI itemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Player Currency UI")]
        [SerializeField] private TextMeshProUGUI goldText;

        private List<WeaponShopItemUI> spawnedItems = new List<WeaponShopItemUI>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseShop);
            }
        }

        private void OnEnable()
        {
            WeaponShopManager.OnWeaponUnlockStateChanged += RefreshShopUI;
            SaveManager.OnSaveCompleted += RefreshShopUI;
        }

        private void OnDisable()
        {
            WeaponShopManager.OnWeaponUnlockStateChanged -= RefreshShopUI;
            SaveManager.OnSaveCompleted -= RefreshShopUI;
        }

        private void Start()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
        }

        public void OpenShop()
        {
            if (shopPanel != null) shopPanel.SetActive(true);
            PopulateShopItems();
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

        public void PopulateShopItems()
        {
            if (itemContainer == null || itemPrefab == null) return;

            // Xóa items cũ
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }
            spawnedItems.Clear();

            WeaponDatabase db = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.Database : null;
            if (db == null || db.AllWeapons == null)
            {
                Debug.LogWarning("[WeaponShopUIController] Chưa gán WeaponDatabase vào WeaponShopManager!");
                return;
            }

            foreach (WeaponData weapon in db.AllWeapons)
            {
                if (weapon == null) continue;

                WeaponShopItemUI itemUI = Instantiate(itemPrefab, itemContainer);
                itemUI.Setup(weapon, OnBuyWeaponClicked, OnEquipWeaponClicked);
                spawnedItems.Add(itemUI);
            }
        }

        private void OnBuyWeaponClicked(WeaponData weapon)
        {
            if (WeaponShopManager.Instance != null)
            {
                bool success = WeaponShopManager.Instance.TryPurchaseWeapon(weapon);
                if (success)
                {
                    RefreshShopUI();
                }
            }
        }

        private void OnEquipWeaponClicked(WeaponData weapon)
        {
            if (WeaponShopManager.Instance != null)
            {
                bool success = WeaponShopManager.Instance.ToggleEquipSupportWeapon(weapon);
                if (success)
                {
                    RefreshShopUI();
                }
            }
        }

        public void RefreshShopUI()
        {
            UpdateGoldDisplay();
            foreach (var itemUI in spawnedItems)
            {
                if (itemUI != null)
                {
                    itemUI.RefreshUI();
                }
            }
        }

        private void UpdateGoldDisplay()
        {
            if (goldText != null)
            {
                int gold = SaveManager.Instance?.CurrentSaveData?.progressData?.totalCurrency ?? 0;
                goldText.text = $"<color=#ffcc00>{gold}</color> Vàng";
            }
        }
    }
}
