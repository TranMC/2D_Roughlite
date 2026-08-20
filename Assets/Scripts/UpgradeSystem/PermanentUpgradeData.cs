using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// ScriptableObject định nghĩa một Nâng cấp vĩnh viễn với cấu trúc dữ liệu nhiều bậc (Multi-tier).
    /// </summary>
    [CreateAssetMenu(fileName = "NewPermanentUpgrade", menuName = "Roguelite/Upgrade System/Permanent Upgrade Data")]
    public class PermanentUpgradeData : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("ID duy nhất của Nâng cấp này (ví dụ: 'perm_max_health', 'perm_attack_damage').")]
        [SerializeField] private string upgradeId;

        [Tooltip("Tên hiển thị trên UI.")]
        [SerializeField] private string upgradeName;

        [Tooltip("Mô tả công dụng của nâng cấp.")]
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Tooltip("Icon đại diện.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Phân loại nhóm Nâng cấp.")]
        [SerializeField] private PermanentUpgradeCategory category = PermanentUpgradeCategory.Offense;

        [Header("Multi-Tier Configuration")]
        [Tooltip("Danh sách các bậc nâng cấp (Level 1 -> Level Max).")]
        [SerializeField] private List<PermanentUpgradeTier> tiers = new List<PermanentUpgradeTier>();

        // Properties public chỉ đọc
        public string UpgradeId => upgradeId;
        public string UpgradeName => upgradeName;
        public string Description => description;
        public Sprite Icon => icon;
        public PermanentUpgradeCategory Category => category;
        public List<PermanentUpgradeTier> Tiers => tiers;
        public int MaxLevel => tiers != null ? tiers.Count : 0;

        /// <summary>
        /// Lấy dữ liệu Tier tương ứng với Cấp độ (1-indexed).
        /// Trả về null nếu level vượt quá giới hạn hoặc không hợp lệ.
        /// </summary>
        public PermanentUpgradeTier GetTier(int level)
        {
            if (tiers == null || level <= 0 || level > tiers.Count)
            {
                return null;
            }
            return tiers[level - 1];
        }

        /// <summary>
        /// Lấy chi phí nâng cấp lên cấp tiếp theo.
        /// </summary>
        public int GetCostForNextLevel(int currentLevel)
        {
            int nextLevel = currentLevel + 1;
            PermanentUpgradeTier nextTier = GetTier(nextLevel);
            return nextTier != null ? nextTier.cost : -1;
        }
    }
}
