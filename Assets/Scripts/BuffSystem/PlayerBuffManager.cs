using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.BuffSystem
{
    /// <summary>
    /// Quản lý buff visual trên player. Hiện tại chỉ xử lý hiển thị icon, chưa áp dụng hiệu ứng gameplay.
    /// </summary>
    [RequireComponent(typeof(PlayerBuffDisplay))]
    public class PlayerBuffManager : MonoBehaviour
    {
        [SerializeField] private PlayerBuffDisplay buffDisplay;

        private readonly List<ActiveBuffInstance> activeBuffs = new List<ActiveBuffInstance>();
        private Player.PlayerStats playerStats;
        private float regenTickTimer = 0f;

        private void Awake()
        {
            if (buffDisplay == null)
            {
                buffDisplay = GetComponent<PlayerBuffDisplay>();
            }

            playerStats = GetComponent<Player.PlayerStats>();
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<Player.PlayerStats>();
            }
        }

        private void Update()
        {
            if (activeBuffs.Count == 0)
            {
                regenTickTimer = 0f;
                return;
            }

            bool changed = false;

            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                ActiveBuffInstance instance = activeBuffs[i];
                instance.RemainingTime -= Time.deltaTime;

                if (instance.RemainingTime <= 0f)
                {
                    activeBuffs.RemoveAt(i);
                    changed = true;
                }
                else
                {
                    activeBuffs[i] = instance;
                }
            }

            // Xử lý hiệu ứng hồi máu theo nhịp thời gian (Regeneration)
            float regenRate = GetTotalHealthRegenRate();
            if (regenRate > 0f && playerStats != null && !playerStats.IsDead && playerStats.CurrentHealth < playerStats.MaxHealth)
            {
                regenTickTimer += Time.deltaTime;
                if (regenTickTimer >= 1f)
                {
                    regenTickTimer -= 1f;
                    playerStats.Heal(regenRate);
                }
            }
            else
            {
                regenTickTimer = 0f;
            }

            buffDisplay.UpdateBuffTimers(BuildDisplayInfos());

            if (changed)
            {
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Nhận buff từ buff zone. Mỗi loại buff chỉ hiển thị một icon, nhưng vẫn đếm số lần nhặt.
        /// </summary>
        public void ApplyBuff(BuffDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            activeBuffs.Add(new ActiveBuffInstance(definition, definition.Duration));
            RefreshDisplay();
        }

        /// <summary>
        /// Nhận buff tùy chỉnh (như Perk last_stand, hiệu ứng kĩ năng, v.v.).
        /// </summary>
        public void ApplyBuff(string buffId, Sprite icon, float duration)
        {
            if (icon == null || duration <= 0f)
            {
                return;
            }

            activeBuffs.Add(new ActiveBuffInstance(buffId, icon, duration));
            RefreshDisplay();
        }

        public int GetBuffStackCount(BuffType buffType)
        {
            int count = 0;
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].Definition != null && activeBuffs[i].Definition.BuffType == buffType)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Tổng tỷ lệ % tăng sát thương từ các buff DamageBoost đang có (ví dụ 0.3 = +30%).
        /// </summary>
        public float GetTotalDamageBonusPercent()
        {
            float total = 0f;
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].Definition != null && activeBuffs[i].Definition.BuffType == BuffType.DamageBoost)
                {
                    total += activeBuffs[i].Definition.EffectValue;
                }
            }
            return total;
        }

        /// <summary>
        /// Tổng tỷ lệ % giảm sát thương nhận vào từ các buff DamageReduction đang có (ví dụ 0.3 = giảm 30%).
        /// Giới hạn tối đa ở 80% (0.8f) để tránh miễn nhiễm tuyệt đối nếu nhặt quá nhiều.
        /// </summary>
        public float GetTotalDamageReductionPercent()
        {
            float total = 0f;
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].Definition != null && activeBuffs[i].Definition.BuffType == BuffType.DamageReduction)
                {
                    total += activeBuffs[i].Definition.EffectValue;
                }
            }
            return Mathf.Clamp(total, 0f, 0.8f);
        }

        /// <summary>
        /// Tổng lượng máu hồi phục mỗi giây từ các buff Regeneration đang có (HP/s).
        /// </summary>
        public float GetTotalHealthRegenRate()
        {
            float total = 0f;
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].Definition != null && activeBuffs[i].Definition.BuffType == BuffType.Regeneration)
                {
                    total += activeBuffs[i].Definition.EffectValue;
                }
            }
            return total;
        }

        private void RefreshDisplay()
        {
            buffDisplay.SetActiveBuffs(BuildDisplayInfos());
        }

        private List<BuffDisplayInfo> BuildDisplayInfos()
        {
            List<BuffDisplayInfo> displayInfos = new List<BuffDisplayInfo>();
            HashSet<string> seenKeys = new HashSet<string>();

            for (int i = 0; i < activeBuffs.Count; i++)
            {
                ActiveBuffInstance instance = activeBuffs[i];
                string uniqueKey = instance.UniqueKey;

                if (!seenKeys.Add(uniqueKey))
                {
                    continue;
                }

                float shortestRemaining = instance.RemainingTime;
                float totalDuration = instance.TotalDuration;
                Sprite icon = instance.Icon;
                BuffDefinition def = instance.Definition;

                for (int j = i + 1; j < activeBuffs.Count; j++)
                {
                    ActiveBuffInstance other = activeBuffs[j];
                    if (other.UniqueKey != uniqueKey)
                    {
                        continue;
                    }

                    if (other.RemainingTime < shortestRemaining)
                    {
                        shortestRemaining = other.RemainingTime;
                    }
                }

                displayInfos.Add(new BuffDisplayInfo(icon, shortestRemaining, totalDuration, uniqueKey, def));
            }

            return displayInfos;
        }

        private struct ActiveBuffInstance
        {
            public string BuffId;
            public Sprite CustomIcon;
            public BuffDefinition Definition;
            public float RemainingTime;
            public float TotalDuration;

            public string UniqueKey => Definition != null ? $"BuffType_{Definition.BuffType}" : BuffId;
            public Sprite Icon => Definition != null ? Definition.Icon : CustomIcon;

            public ActiveBuffInstance(BuffDefinition definition, float remainingTime)
            {
                BuffId = string.Empty;
                CustomIcon = null;
                Definition = definition;
                RemainingTime = remainingTime;
                TotalDuration = definition != null ? definition.Duration : remainingTime;
            }

            public ActiveBuffInstance(string buffId, Sprite icon, float duration)
            {
                BuffId = buffId;
                CustomIcon = icon;
                Definition = null;
                RemainingTime = duration;
                TotalDuration = duration;
            }
        }
    }
}
