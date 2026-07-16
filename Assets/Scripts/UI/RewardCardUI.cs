using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.UpgradeSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Hiển thị thông tin của một Perk lên Card UI.
    /// </summary>
    public class RewardCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image borderImage;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color rareColor = new Color(0.2f, 0.6f, 1f); // Xanh dương
        [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 1f); // Tím
        [SerializeField] private Color legendaryColor = new Color(1f, 0.6f, 0f); // Cam

        private PerkData currentPerkData;
        public PerkData CurrentPerkData => currentPerkData;

        /// <summary>
        /// Gán dữ liệu Perk và cập nhật giao diện hiển thị.
        /// </summary>
        public void SetupCard(PerkData data)
        {
            currentPerkData = data;

            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Gán Text & Icon
            if (nameText != null) nameText.text = data.PerkName;
            if (descriptionText != null) descriptionText.text = data.Description;
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
            }

            // Đổi màu viền dựa theo độ hiếm
            if (borderImage != null)
            {
                borderImage.color = GetRarityColor(data.Rarity);
            }
        }

        /// <summary>
        /// Lấy mã màu tương ứng với độ hiếm.
        /// </summary>
        private Color GetRarityColor(PerkRarity rarity)
        {
            switch (rarity)
            {
                case PerkRarity.Common: return commonColor;
                case PerkRarity.Rare: return rareColor;
                case PerkRarity.Epic: return epicColor;
                case PerkRarity.Legendary: return legendaryColor;
                default: return Color.white;
            }
        }
    }
}
