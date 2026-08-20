using System;
using UnityEngine;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Thông tin mốc thưởng đặc biệt khi đạt tới một Cấp bậc (Milestone) nhất định.
    /// </summary>
    [Serializable]
    public struct MilestoneBonusData
    {
        [Tooltip("Mô tả ngắn về bonus mốc đặc biệt này.")]
        public string bonusDescription;

        [Tooltip("Loại chỉ số nhận thêm làm bonus mốc.")]
        public PlayerStatType statType;

        [Tooltip("Giá trị bonus cộng thêm.")]
        public float statValue;

        [Tooltip("Là tỷ lệ phần trăm % hay giá trị cộng thẳng.")]
        public bool isPercent;

        [Tooltip("Mã hiệu ứng đặc biệt (nếu có, ví dụ: 'last_stand', 'gold_boost').")]
        public string specialBehaviorKey;
    }

    /// <summary>
    /// Định nghĩa chi tiết một Cấp bậc (Tier/Level) của Permanent Upgrade.
    /// </summary>
    [Serializable]
    public class PermanentUpgradeTier
    {
        [Tooltip("Cấp bậc (1-indexed: 1, 2, 3...)")]
        public int tierIndex = 1;

        [Tooltip("Chi phí vàng / linh hồn để nâng cấp lên bậc này.")]
        public int cost = 100;

        [Tooltip("Loại chỉ số bị ảnh hưởng bởi nâng cấp này.")]
        public PlayerStatType statType = PlayerStatType.MaxHealth;

        [Tooltip("Giá trị chỉ số tăng thêm ở bậc này (Flat hoặc Percent).")]
        public float statValue = 10f;

        [Tooltip("Đây có phải là giá trị phần trăm (%) hay không.")]
        public bool isPercent = false;

        [Header("Milestone Settings")]
        [Tooltip("Đánh dấu đây là Cấp bậc mốc đặc biệt (Milestone Tier).")]
        public bool isMilestone = false;

        [Tooltip("Chi tiết phần thưởng mốc đặc biệt.")]
        public MilestoneBonusData milestoneBonus;
    }
}
