using System;
using System.Collections.Generic;
using UnityEngine;
using Roguelite.Combat;
using Roguelite.Enemy;
using Roguelite.Player;
using Roguelite.SaveSystem;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Singleton Quản lý toàn bộ hệ thống Nâng cấp Vĩnh viễn (Permanent Upgrade System),
    /// xử lý mua nâng cấp, kiểm tra & cấp bonus mốc đặc biệt tránh trùng lặp,
    /// tích hợp SaveManager và áp dụng chỉ số vĩnh viễn lên PlayerStats khi bắt đầu Run.
    /// </summary>
    public class PermanentUpgradeManager : MonoBehaviour
    {
        public const string VERSION = "1.3.0";
        private static PermanentUpgradeManager instance;
        public static PermanentUpgradeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PermanentUpgradeManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("[PermanentUpgradeManager]");
                        instance = go.AddComponent<PermanentUpgradeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        [Header("Database Configuration")]
        [Tooltip("Database chứa tất cả Permanent Upgrade ScriptableObjects.")]
        [SerializeField] private PermanentUpgradeDatabase database;

        // Events
        public static event Action<PermanentUpgradeData, int> OnUpgradePurchased;
        public static event Action<string, MilestoneBonusData> OnMilestoneBonusGranted;
        public static event Action<int> OnCurrencyChanged;
        public static event Action OnPermanentStatsApplied;

        public PermanentUpgradeDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = GetOrLoadPermanentUpgradeDatabase();
                }
                return database;
            }
            set => database = value;
        }

        public static PermanentUpgradeDatabase GetOrLoadPermanentUpgradeDatabase()
        {
            PermanentUpgradeDatabase db = Resources.Load<PermanentUpgradeDatabase>("PermanentUpgradeDatabase");
            if (db == null)
            {
                var dbs = Resources.FindObjectsOfTypeAll<PermanentUpgradeDatabase>();
                if (dbs != null && dbs.Length > 0) db = dbs[0];
            }
#if UNITY_EDITOR
            if (db == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:PermanentUpgradeDatabase");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    db = UnityEditor.AssetDatabase.LoadAssetAtPath<PermanentUpgradeDatabase>(path);
                }
            }
#endif
            return db;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            EnemyBase.OnAnyEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            EnemyBase.OnAnyEnemyDied -= HandleEnemyDied;
        }

        /// <summary>
        /// Xử lý tự động thưởng vàng/linh hồn cho Player khi tiêu diệt quái vật.
        /// </summary>
        private void HandleEnemyDied(EnemyBase enemy)
        {
            if (enemy == null) return;

            int rewardAmount = enemy.CurrencyReward;
            AddCurrency(rewardAmount);

            // Cập nhật số quái đã diệt vào SaveData
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                SaveManager.Instance.CurrentSaveData.progressData.totalEnemiesKilled++;
            }
        }

        /// <summary>
        /// Cộng tiền (vàng/linh hồn) cho người chơi.
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;

            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                SaveManager.Instance.CurrentSaveData.progressData.totalCurrency += amount;
                int currentTotal = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
                
                Debug.Log($"[PermanentUpgradeManager] +{amount} Gold! Tổng tiền hiện tại: {currentTotal}");
                OnCurrencyChanged?.Invoke(currentTotal);
            }
        }

        /// <summary>
        /// Trừ tiền người chơi (khi mua sắm/nâng cấp).
        /// </summary>
        public bool SpendCurrency(int amount)
        {
            if (amount <= 0) return true;

            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            {
                return false;
            }

            int currentCurrency = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
            if (currentCurrency >= amount)
            {
                SaveManager.Instance.CurrentSaveData.progressData.totalCurrency -= amount;
                int newTotal = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;

                Debug.Log($"[PermanentUpgradeManager] Trừ {amount} Gold. Tiền còn lại: {newTotal}");
                OnCurrencyChanged?.Invoke(newTotal);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lấy Cấp độ nâng cấp vĩnh viễn hiện tại của 1 UpgradeId từ SaveData.
        /// </summary>
        public int GetUpgradeLevel(string upgradeId)
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            {
                return 0;
            }
            return SaveManager.Instance.CurrentSaveData.abilityData.GetAbilityLevel(upgradeId);
        }

        /// <summary>
        /// Kiểm tra xem người chơi đã đạt đủ tất cả các điều kiện mở bán (Diệt quái, Runs, Rooms) hay chưa.
        /// </summary>
        public bool IsRequirementMet(PermanentUpgradeData upgradeData)
        {
            if (upgradeData == null) return false;
            if (upgradeData.IsDefaultUnlocked) return true;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return false;

            return upgradeData.IsRequirementMet(SaveManager.Instance.CurrentSaveData.progressData);
        }

        /// <summary>
        /// Kiểm tra xem người chơi có đủ điều kiện mở bán và đủ tiền mua Cấp tiếp theo không.
        /// </summary>
        public bool CanAffordUpgrade(PermanentUpgradeData upgradeData)
        {
            if (upgradeData == null) return false;
            if (!IsRequirementMet(upgradeData)) return false;

            int currentLevel = GetUpgradeLevel(upgradeData.UpgradeId);
            if (currentLevel >= upgradeData.MaxLevel) return false;

            int cost = upgradeData.GetCostForNextLevel(currentLevel);
            if (cost < 0) return false;

            int currentCurrency = SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null
                ? SaveManager.Instance.CurrentSaveData.progressData.totalCurrency
                : 0;

            return currentCurrency >= cost;
        }

        /// <summary>
        /// Thực hiện Mua / Nâng cấp 1 bậc cho Permanent Upgrade.
        /// </summary>
        public bool TryPurchaseUpgrade(PermanentUpgradeData upgradeData)
        {
            if (upgradeData == null) return false;

            if (!IsRequirementMet(upgradeData))
            {
                Debug.LogWarning($"[PermanentUpgradeManager] Chưa đạt đủ điều kiện mở bán cho '{upgradeData.UpgradeName}'!");
                return false;
            }

            string upgradeId = upgradeData.UpgradeId;
            int currentLevel = GetUpgradeLevel(upgradeId);

            if (currentLevel >= upgradeData.MaxLevel)
            {
                Debug.LogWarning($"[PermanentUpgradeManager] Upgrade '{upgradeData.UpgradeName}' đã đạt level tối đa ({upgradeData.MaxLevel})!");
                return false;
            }

            int nextLevel = currentLevel + 1;
            PermanentUpgradeTier nextTier = upgradeData.GetTier(nextLevel);
            if (nextTier == null) return false;

            // Kiểm tra và trừ tiền
            if (!SpendCurrency(nextTier.cost))
            {
                Debug.LogWarning($"[PermanentUpgradeManager] Không đủ tiền để mua '{upgradeData.UpgradeName}' Level {nextLevel} (Cần {nextTier.cost} Gold)!");
                return false;
            }

            // Cập nhật Cấp độ mới vào SaveData
            SaveManager.Instance.CurrentSaveData.abilityData.SetAbilityLevel(upgradeId, nextLevel);
            Debug.Log($"[PermanentUpgradeManager] Nâng cấp thành công '{upgradeData.UpgradeName}' lên Level {nextLevel}!");

            // Detect & Grant Milestone Bonus đặc biệt (Tránh trùng lặp)
            CheckAndGrantMilestone(upgradeData, nextTier);

            // Tự động lưu tiến trình
            SaveManager.Instance.TriggerAutoSave(0.5f);

            OnUpgradePurchased?.Invoke(upgradeData, nextLevel);
            return true;
        }

        /// <summary>
        /// Logic phát hiện và cấp Bonus ở các mốc Milestone đặc biệt, tránh nhận trùng lặp.
        /// </summary>
        private void CheckAndGrantMilestone(PermanentUpgradeData upgradeData, PermanentUpgradeTier tier)
        {
            if (tier == null || !tier.isMilestone) return;

            string milestoneKey = $"{upgradeData.UpgradeId}_milestone_tier_{tier.tierIndex}";
            AbilityUnlockData abilityData = SaveManager.Instance.CurrentSaveData.abilityData;

            // Kiểm tra xem milestone này đã được cấp chưa
            if (abilityData.IsMilestoneGranted(milestoneKey))
            {
                Debug.Log($"[PermanentUpgradeManager] Milestone '{milestoneKey}' đã từng được nhận trước đó. Bỏ qua để tránh trùng lặp.");
                return;
            }

            // Đánh dấu đã nhận milestone trong SaveData
            abilityData.MarkMilestoneGranted(milestoneKey);
            Debug.Log($"[PermanentUpgradeManager] 🎉 ĐẠT MỐC MILESTONE! Đã nhận thưởng mốc cho '{upgradeData.UpgradeName}' Tier {tier.tierIndex}: {tier.milestoneBonus.bonusDescription}");

            OnMilestoneBonusGranted?.Invoke(milestoneKey, tier.milestoneBonus);
        }


        /// <summary>
        /// Thu thập tất cả các modifier nâng cấp vĩnh viễn đã mở khóa vào 5 nhóm StatModifierGroup.
        /// </summary>
        public void CollectPermanentModifiers(
            ref StatModifierGroup hpGroup,
            ref StatModifierGroup walkSpeedGroup,
            ref StatModifierGroup runSpeedGroup,
            ref StatModifierGroup jumpGroup,
            ref StatModifierGroup damageGroup)
        {
            if (database == null || database.AllUpgrades == null) return;
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null) return;

            foreach (var upgrade in database.AllUpgrades)
            {
                if (upgrade == null) continue;

                int unlockedLevel = GetUpgradeLevel(upgrade.UpgradeId);
                if (unlockedLevel <= 0) continue;

                for (int level = 1; level <= unlockedLevel; level++)
                {
                    PermanentUpgradeTier tier = upgrade.GetTier(level);
                    if (tier == null) continue;

                    AccumulateStatModifier(tier.statType, tier.statValue, tier.isPercent,
                        ref hpGroup, ref walkSpeedGroup, ref runSpeedGroup, ref jumpGroup, ref damageGroup);

                    if (tier.isMilestone)
                    {
                        string milestoneKey = $"{upgrade.UpgradeId}_milestone_tier_{tier.tierIndex}";
                        if (SaveManager.Instance.CurrentSaveData.abilityData.IsMilestoneGranted(milestoneKey))
                        {
                            AccumulateStatModifier(tier.milestoneBonus.statType, tier.milestoneBonus.statValue, tier.milestoneBonus.isPercent,
                                ref hpGroup, ref walkSpeedGroup, ref runSpeedGroup, ref jumpGroup, ref damageGroup);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Áp dụng các hiệu ứng nâng cấp vĩnh viễn kết hợp với Active Perks lên Player hiện tại.
        /// </summary>
        public void ApplyAllUpgrades(GameObject player)
        {
            if (player == null) return;

            var activePerks = UpgradeManager.Instance != null ? UpgradeManager.Instance.ActivePerks : null;
            PerkEffectApplier.ApplyCombinedStats(player, activePerks);

            OnPermanentStatsApplied?.Invoke();
        }

        private void AccumulateStatModifier(PlayerStatType statType, float value, bool isPercent,
            ref StatModifierGroup hpGroup, ref StatModifierGroup walkSpeedGroup,
            ref StatModifierGroup runSpeedGroup, ref StatModifierGroup jumpGroup,
            ref StatModifierGroup damageGroup)
        {
            switch (statType)
            {
                case PlayerStatType.MaxHealth:
                    if (isPercent) hpGroup.AddPercentAdditive(value);
                    else hpGroup.AddFlat(value);
                    break;
                case PlayerStatType.WalkSpeed:
                    if (isPercent) walkSpeedGroup.AddPercentAdditive(value);
                    else walkSpeedGroup.AddFlat(value);
                    break;
                case PlayerStatType.RunSpeed:
                    if (isPercent) runSpeedGroup.AddPercentAdditive(value);
                    else runSpeedGroup.AddFlat(value);
                    break;
                case PlayerStatType.JumpImpulse:
                    if (isPercent) jumpGroup.AddPercentAdditive(value);
                    else jumpGroup.AddFlat(value);
                    break;
                case PlayerStatType.AttackDamage:
                    if (isPercent) damageGroup.AddPercentAdditive(value);
                    else damageGroup.AddFlat(value);
                    break;
            }
        }
    }
}
