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
        /// Tính toán tổng hiệu ứng của các active perks và áp dụng lên các thành phần của Player.
        /// </summary>
        /// <param name="player">GameObject của Player</param>
        /// <param name="activePerks">Danh sách các Perk đang hoạt động và số stack của chúng</param>
        public static void ApplyAllActivePerks(GameObject player, Dictionary<PerkData, int> activePerks)
        {
            if (player == null) return;

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerStats == null || playerController == null)
            {
                Debug.LogError("[PerkEffectApplier] Không tìm thấy PlayerStats hoặc PlayerController trên Player GameObject!");
                return;
            }

            // --- 1. Khởi tạo các biến tích lũy modifiers cho từng chỉ số ---
            float maxHealthFlat = 0f;
            float maxHealthPercent = 0f;

            float walkSpeedFlat = 0f;
            float walkSpeedPercent = 0f;
            float runSpeedFlat = 0f;
            float runSpeedPercent = 0f;

            float jumpFlat = 0f;
            float jumpPercent = 0f;

            float damageFlat = 0f;
            float damagePercent = 0f;

            // --- 2. Duyệt qua danh sách các Perk đang active để tích lũy modifiers ---
            foreach (var kvp in activePerks)
            {
                PerkData perk = kvp.Key;
                int stackCount = kvp.Value;

                if (perk == null || stackCount <= 0) continue;

                // Bỏ qua nếu không phải StatModifier (SpecialBehavior sẽ được xử lý riêng theo event)
                if (perk.EffectType != PerkEffectType.StatModifier) continue;

                // Tính toán giá trị tổng hợp của perk này dựa trên StackBehavior
                float totalValue = CalculateTotalValue(perk.EffectValue, stackCount, perk.StackBehavior, perk.IsPercent);

                // Gom nhóm các modifier theo loại chỉ số
                switch (perk.StatType)
                {
                    case PlayerStatType.MaxHealth:
                        if (perk.IsPercent) maxHealthPercent += totalValue;
                        else maxHealthFlat += totalValue;
                        break;

                    case PlayerStatType.WalkSpeed:
                        if (perk.IsPercent) walkSpeedPercent += totalValue;
                        else walkSpeedFlat += totalValue;
                        break;

                    case PlayerStatType.RunSpeed:
                        if (perk.IsPercent) runSpeedPercent += totalValue;
                        else runSpeedFlat += totalValue;
                        break;

                    case PlayerStatType.JumpImpulse:
                        if (perk.IsPercent) jumpPercent += totalValue;
                        else jumpFlat += totalValue;
                        break;

                    case PlayerStatType.AttackDamage:
                        if (perk.IsPercent) damagePercent += totalValue;
                        else damageFlat += totalValue;
                        break;
                }
            }

            // --- 3. Áp dụng các thay đổi lên Player ---
            // Nâng HP
            playerStats.ApplyMaxHealthModifier(maxHealthFlat, maxHealthPercent);

            // Nâng Tốc độ di chuyển
            playerController.ApplySpeedModifiers(walkSpeedFlat, walkSpeedPercent, runSpeedFlat, runSpeedPercent);

            // Nâng Lực nhảy
            playerController.ApplyJumpModifiers(jumpFlat, jumpPercent);

            // Nâng Sát thương: tìm tất cả component Attack con trên Player và áp dụng
            Attack[] attacks = player.GetComponentsInChildren<Attack>(true);
            foreach (Attack attack in attacks)
            {
                attack.ApplyDamageModifier(damageFlat, damagePercent);
            }

            Debug.Log($"[PerkEffectApplier] Đã áp dụng lại toàn bộ active perks lên Player. " +
                      $"HP Bonus: +{maxHealthFlat}/+{maxHealthPercent*100}%, " +
                      $"WalkSpeed Bonus: +{walkSpeedFlat}/+{walkSpeedPercent*100}%, " +
                      $"Damage Bonus: +{damageFlat}/+{damagePercent*100}%");
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
