using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Bể chứa toàn bộ các Perk có sẵn trong game.
    /// Dùng làm cấu hình cho hệ thống Upgrade System.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPerkPool", menuName = "Roguelite/Upgrade System/Perk Pool")]
    public class PerkPool : ScriptableObject
    {
        [Tooltip("Danh sách toàn bộ các Perk có sẵn để rút ngẫu nhiên.")]
        [SerializeField] private List<PerkData> allPerks = new List<PerkData>();

        /// <summary>
        /// Danh sách toàn bộ các Perk có trong pool.
        /// </summary>
        public List<PerkData> AllPerks => allPerks;
    }
}
