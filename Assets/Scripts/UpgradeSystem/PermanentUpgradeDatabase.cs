using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Database lưu trữ tập hợp toàn bộ ScriptableObjects của Permanent Upgrades.
    /// </summary>
    [CreateAssetMenu(fileName = "PermanentUpgradeDatabase", menuName = "Roguelite/Upgrade System/Permanent Upgrade Database")]
    public class PermanentUpgradeDatabase : ScriptableObject
    {
        [SerializeField] private List<PermanentUpgradeData> allUpgrades = new List<PermanentUpgradeData>();

        public List<PermanentUpgradeData> AllUpgrades => allUpgrades;

        /// <summary>
        /// Tìm Nâng cấp theo upgradeId.
        /// </summary>
        public PermanentUpgradeData GetUpgradeById(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId) || allUpgrades == null) return null;
            return allUpgrades.Find(u => u != null && u.UpgradeId == upgradeId);
        }

        /// <summary>
        /// Lọc danh sách nâng cấp theo danh mục (Category).
        /// </summary>
        public List<PermanentUpgradeData> GetUpgradesByCategory(PermanentUpgradeCategory category)
        {
            if (category == PermanentUpgradeCategory.All)
            {
                return new List<PermanentUpgradeData>(allUpgrades);
            }
            return allUpgrades.FindAll(u => u != null && u.Category == category);
        }
    }
}
