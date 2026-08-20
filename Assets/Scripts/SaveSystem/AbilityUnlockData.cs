using System;
using System.Collections.Generic;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Một phần tử lưu cấp độ nâng cấp cho Ability/Perk vĩnh viễn.
    /// </summary>
    [Serializable]
    public struct AbilityLevelEntry
    {
        public string abilityId;
        public int currentLevel;

        public AbilityLevelEntry(string id, int level)
        {
            abilityId = id;
            currentLevel = level;
        }
    }

    /// <summary>
    /// Dữ liệu các Ability/Perk vĩnh viễn đã được nâng cấp.
    /// </summary>
    [Serializable]
    public class AbilityUnlockData
    {
        public List<AbilityLevelEntry> abilityLevels = new List<AbilityLevelEntry>();
        public List<string> grantedMilestones = new List<string>();

        public AbilityUnlockData()
        {
            abilityLevels = new List<AbilityLevelEntry>();
            grantedMilestones = new List<string>();
        }

        public int GetAbilityLevel(string abilityId)
        {
            for (int i = 0; i < abilityLevels.Count; i++)
            {
                if (abilityLevels[i].abilityId == abilityId)
                {
                    return abilityLevels[i].currentLevel;
                }
            }
            return 0;
        }

        public void SetAbilityLevel(string abilityId, int level)
        {
            for (int i = 0; i < abilityLevels.Count; i++)
            {
                if (abilityLevels[i].abilityId == abilityId)
                {
                    var entry = abilityLevels[i];
                    entry.currentLevel = level;
                    abilityLevels[i] = entry;
                    return;
                }
            }
            abilityLevels.Add(new AbilityLevelEntry(abilityId, level));
        }

        public bool IsMilestoneGranted(string milestoneKey)
        {
            if (grantedMilestones == null) return false;
            return grantedMilestones.Contains(milestoneKey);
        }

        public void MarkMilestoneGranted(string milestoneKey)
        {
            if (grantedMilestones == null)
            {
                grantedMilestones = new List<string>();
            }
            if (!grantedMilestones.Contains(milestoneKey))
            {
                grantedMilestones.Add(milestoneKey);
            }
        }
    }
}
