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

        public PermanentUpgradeDatabase Database => database;

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
        /// Áp dụng toàn bộ chỉ số Nâng cấp vĩnh viễn (+ Milestone bonuses) lên Player khi bắt đầu Run.
        /// </summary>
        public void ApplyAllUpgrades(GameObject player)
        {
            if (player == null) return;

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerStats == null || playerController == null)
            {
                Debug.LogError("[PermanentUpgradeManager] Không tìm thấy PlayerStats hoặc PlayerController trên Player GameObject!");
                return;
            }

            if (database == null || database.AllUpgrades == null)
            {
                Debug.LogWarning("[PermanentUpgradeManager] Chưa gán PermanentUpgradeDatabase!");
                return;
            }

            // Dùng StatModifierGroup để gom nhóm modifiers và giải quyết xung đột chỉ số
            StatModifierGroup hpGroup = StatModifierGroup.Default;
            StatModifierGroup walkSpeedGroup = StatModifierGroup.Default;
            StatModifierGroup runSpeedGroup = StatModifierGroup.Default;
            StatModifierGroup jumpGroup = StatModifierGroup.Default;
            StatModifierGroup damageGroup = StatModifierGroup.Default;

            // 1. Duyệt qua tất cả Permanent Upgrades trong Database
            foreach (var upgrade in database.AllUpgrades)
            {
                if (upgrade == null) continue;

                int unlockedLevel = GetUpgradeLevel(upgrade.UpgradeId);
                if (unlockedLevel <= 0) continue;

                // Tích lũy hiệu ứng từ Tier 1 -> unlockedLevel
                for (int level = 1; level <= unlockedLevel; level++)
                {
                    PermanentUpgradeTier tier = upgrade.GetTier(level);
                    if (tier == null) continue;

                    AccumulateStatModifier(tier.statType, tier.statValue, tier.isPercent,
                        ref hpGroup, ref walkSpeedGroup, ref runSpeedGroup, ref jumpGroup, ref damageGroup);

                    // Nếu tier có Milestone và đã được granted -> tích lũy thêm milestone bonus
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

            // 2. Áp dụng chỉ số đã được tính toán thông qua StatCalculator
            // Áp dụng Max Health
            playerStats.ApplyMaxHealthModifier(hpGroup.flatSum, hpGroup.percentAdditiveSum);

            // Áp dụng Speed
            playerController.ApplySpeedModifiers(walkSpeedGroup.flatSum, walkSpeedGroup.percentAdditiveSum,
                                                 runSpeedGroup.flatSum, runSpeedGroup.percentAdditiveSum);

            // Áp dụng Jump
            playerController.ApplyJumpModifiers(jumpGroup.flatSum, jumpGroup.percentAdditiveSum);

            // Áp dụng Sát thương đòn đánh
            Attack[] attacks = player.GetComponentsInChildren<Attack>(true);
            foreach (Attack attack in attacks)
            {
                attack.ApplyDamageModifier(damageGroup.flatSum, damageGroup.percentAdditiveSum);
            }

            Debug.Log($"[PermanentUpgradeManager] 🚀 Đã áp dụng toàn bộ chỉ số Nâng cấp Vĩnh Viễn lên Player! " +
                      $"HP Flat/Perc: +{hpGroup.flatSum}/+{hpGroup.percentAdditiveSum * 100}%, " +
                      $"WalkSpeed Flat/Perc: +{walkSpeedGroup.flatSum}/+{walkSpeedGroup.percentAdditiveSum * 100}%, " +
                      $"Damage Flat/Perc: +{damageGroup.flatSum}/+{damageGroup.percentAdditiveSum * 100}%");

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
