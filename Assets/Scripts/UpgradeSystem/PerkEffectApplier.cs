using System.Collections.Generic;
using UnityEngine;
using Roguelite.Combat;
using Roguelite.Player;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Chịu trách nhiệm tính toán tổng giá trị hiệu ứng từ danh sách active perks
    /// và trực tiếp áp dụng/cập nhật lên các chỉ số của Player.
    /// </summary>
    public static class PerkEffectApplier
    {
        /// <summary>
        /// Tính toán tổng hiệu ứng của các active perks và áp dụng lên các thành phần của Player (gọi chuyển tiếp sang ApplyCombinedStats).
        /// </summary>
        /// <param name="player">GameObject của Player</param>
        /// <param name="activePerks">Danh sách các Perk đang hoạt động và số stack của chúng</param>
        public static void ApplyAllActivePerks(GameObject player, Dictionary<PerkData, int> activePerks)
        {
            ApplyCombinedStats(player, activePerks);
        }

        /// <summary>
        /// Hợp nhất toàn bộ Modifiers từ Permanent Upgrade và Run Perks, áp dụng đồng thời lên Player.
        /// </summary>
        public static void ApplyCombinedStats(GameObject player, Dictionary<PerkData, int> activePerks)
        {
            if (player == null) return;

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerStats == null || playerController == null)
            {
                Debug.LogError("[PerkEffectApplier] Không tìm thấy PlayerStats hoặc PlayerController trên Player GameObject!");
                return;
            }

            // --- 1. Khởi tạo các nhóm modifiers cho từng chỉ số ---
            StatModifierGroup hpGroup = StatModifierGroup.Default;
            StatModifierGroup walkSpeedGroup = StatModifierGroup.Default;
            StatModifierGroup runSpeedGroup = StatModifierGroup.Default;
            StatModifierGroup jumpGroup = StatModifierGroup.Default;
            StatModifierGroup damageGroup = StatModifierGroup.Default;

            // --- 2. Thu thập Modifiers từ Permanent Upgrade (Nâng cấp vĩnh viễn) ---
            if (PermanentUpgradeManager.Instance != null)
            {
                PermanentUpgradeManager.Instance.CollectPermanentModifiers(
                    ref hpGroup, ref walkSpeedGroup, ref runSpeedGroup, ref jumpGroup, ref damageGroup);
            }

            // --- 3. Thu thập Modifiers từ Run Perks (Perk trong trận) ---
            if (activePerks != null)
            {
                foreach (var kvp in activePerks)
                {
                    PerkData perk = kvp.Key;
                    int stackCount = kvp.Value;

                    if (perk == null || stackCount <= 0) continue;
                    if (perk.EffectType != PerkEffectType.StatModifier) continue;

                    float totalValue = CalculateTotalValue(perk.EffectValue, stackCount, perk.StackBehavior, perk.IsPercent);

                    switch (perk.StatType)
                    {
                        case PlayerStatType.MaxHealth:
                            if (perk.IsPercent) hpGroup.AddPercentAdditive(totalValue);
                            else hpGroup.AddFlat(totalValue);
                            break;

                        case PlayerStatType.WalkSpeed:
                            if (perk.IsPercent) walkSpeedGroup.AddPercentAdditive(totalValue);
                            else walkSpeedGroup.AddFlat(totalValue);
                            break;

                        case PlayerStatType.RunSpeed:
                            if (perk.IsPercent) runSpeedGroup.AddPercentAdditive(totalValue);
                            else runSpeedGroup.AddFlat(totalValue);
                            break;

                        case PlayerStatType.JumpImpulse:
                            if (perk.IsPercent) jumpGroup.AddPercentAdditive(totalValue);
                            else jumpGroup.AddFlat(totalValue);
                            break;

                        case PlayerStatType.AttackDamage:
                            if (perk.IsPercent) damageGroup.AddPercentAdditive(totalValue);
                            else damageGroup.AddFlat(totalValue);
                            break;
                    }
                }
            }

            // --- 4. Áp dụng các thay đổi hợp nhất lên Player ---
            playerStats.ApplyMaxHealthModifier(hpGroup.flatSum, hpGroup.percentAdditiveSum);

            playerController.ApplySpeedModifiers(
                walkSpeedGroup.flatSum, walkSpeedGroup.percentAdditiveSum,
                runSpeedGroup.flatSum, runSpeedGroup.percentAdditiveSum);

            playerController.ApplyJumpModifiers(jumpGroup.flatSum, jumpGroup.percentAdditiveSum);

            Attack[] attacks = player.GetComponentsInChildren<Attack>(true);
            foreach (Attack attack in attacks)
            {
                attack.ApplyDamageModifier(damageGroup.flatSum, damageGroup.percentAdditiveSum);
            }

            Debug.Log($"[PerkEffectApplier] ✅ Đã áp dụng chỉ số kết hợp (Permanent + Perks) lên Player: " +
                      $"HP (+{hpGroup.flatSum}/+{hpGroup.percentAdditiveSum * 100}%), " +
                      $"WalkSpeed (+{walkSpeedGroup.flatSum}/+{walkSpeedGroup.percentAdditiveSum * 100}%), " +
                      $"RunSpeed (+{runSpeedGroup.flatSum}/+{runSpeedGroup.percentAdditiveSum * 100}%), " +
                      $"Jump (+{jumpGroup.flatSum}/+{jumpGroup.percentAdditiveSum * 100}%), " +
                      $"Damage (+{damageGroup.flatSum}/+{damageGroup.percentAdditiveSum * 100}%)");
        }

        /// <summary>
        /// Tính toán giá trị hiệu ứng sau khi cộng dồn (stacking).
        /// </summary>
        private static float CalculateTotalValue(float baseValue, int stackCount, StackBehavior behavior, bool isPercent)
        {
            switch (behavior)
            {
                case StackBehavior.Additive:
                    return baseValue * stackCount;

                case StackBehavior.Multiplicative:
                    // Với Multiplicative: nếu là percent thì nhân dồn lũy thừa: (1 + baseValue)^stackCount - 1
                    if (isPercent)
                    {
                        return Mathf.Pow(1f + baseValue, stackCount) - 1f;
                    }
                    else
                    {
                        // Fallback về Additive đối với cộng thẳng flat
                        return baseValue * stackCount;
                    }

                case StackBehavior.None:
                default:
                    return baseValue;
            }
        }
    }
}
