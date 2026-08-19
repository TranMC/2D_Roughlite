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

        private void Awake()
        {
            if (buffDisplay == null)
            {
                buffDisplay = GetComponent<PlayerBuffDisplay>();
            }
        }

        private void Update()
        {
            if (activeBuffs.Count == 0)
            {
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

        public int GetBuffStackCount(BuffType buffType)
        {
            int count = 0;
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].Definition.BuffType == buffType)
                {
                    count++;
                }
            }

            return count;
        }

        private void RefreshDisplay()
        {
            buffDisplay.SetActiveBuffs(BuildDisplayInfos());
        }

        private List<BuffDisplayInfo> BuildDisplayInfos()
        {
            List<BuffDisplayInfo> displayInfos = new List<BuffDisplayInfo>();
            HashSet<BuffType> seenTypes = new HashSet<BuffType>();

            for (int i = 0; i < activeBuffs.Count; i++)
            {
                ActiveBuffInstance instance = activeBuffs[i];
                BuffType buffType = instance.Definition.BuffType;

                if (!seenTypes.Add(buffType))
                {
                    continue;
                }

                float shortestRemaining = instance.RemainingTime;
                float totalDuration = instance.Definition.Duration;

                for (int j = i + 1; j < activeBuffs.Count; j++)
                {
                    ActiveBuffInstance other = activeBuffs[j];
                    if (other.Definition.BuffType != buffType)
                    {
                        continue;
                    }

                    if (other.RemainingTime < shortestRemaining)
                    {
                        shortestRemaining = other.RemainingTime;
                    }
                }

                displayInfos.Add(new BuffDisplayInfo(instance.Definition, shortestRemaining, totalDuration));
            }

            return displayInfos;
        }

        private struct ActiveBuffInstance
        {
            public BuffDefinition Definition;
            public float RemainingTime;

            public ActiveBuffInstance(BuffDefinition definition, float remainingTime)
            {
                Definition = definition;
                RemainingTime = remainingTime;
            }
        }
    }
}
