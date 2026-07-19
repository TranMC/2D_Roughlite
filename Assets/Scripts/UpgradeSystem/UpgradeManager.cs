using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Roguelite.Core;
using Roguelite.Enemy;
using Roguelite.Player;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Trình quản lý hệ thống nâng cấp (Singleton).
    /// Quản lý active perks, bể perk, thuật toán weighted random và các sự kiện vòng đời.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Perk Pool Configuration")]
        [Tooltip("Bể chứa tất cả Perk trong game.")]
        [SerializeField] private PerkPool perkPool;

        [Header("Weighted Random Settings")]
        [Tooltip("Trọng số xuất hiện của Rarity: Common.")]
        [SerializeField] private int commonWeight = 60;
        [Tooltip("Trọng số xuất hiện của Rarity: Rare.")]
        [SerializeField] private int rareWeight = 25;
        [Tooltip("Trọng số xuất hiện của Rarity: Epic.")]
        [SerializeField] private int epicWeight = 12;
        [Tooltip("Trọng số xuất hiện của Rarity: Legendary.")]
        [SerializeField] private int legendaryWeight = 3;

        // Trạng thái Run hiện tại
        private Dictionary<PerkData, int> activePerks = new Dictionary<PerkData, int>();
        private List<PerkData> lastRunHistory = new List<PerkData>();

        public static event System.Action<PerkData, int> OnPerkAdded;

        // Properties public chỉ đọc
        public Dictionary<PerkData, int> ActivePerks => activePerks;
        public List<PerkData> LastRunHistory => lastRunHistory;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameManager.OnGameStateChanged += OnGameStateChanged;
            EnemyBase.OnAnyEnemyDied += OnEnemyDied;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            EnemyBase.OnAnyEnemyDied -= OnEnemyDied;
        }

        /// <summary>
        /// Thêm hoặc tăng stack cho một Perk.
        /// </summary>
        public void AddPerk(PerkData perk)
        {
            if (perk == null) return;

            if (activePerks.ContainsKey(perk))
            {
                if (activePerks[perk] < perk.MaxStack)
                {
                    activePerks[perk]++;
                    Debug.Log($"[UpgradeManager] Tăng stack Perk '{perk.PerkName}': {activePerks[perk]}/{perk.MaxStack}");
                }
                else
                {
                    Debug.LogWarning($"[UpgradeManager] Perk '{perk.PerkName}' đã đạt stack tối đa!");
                    return;
                }
            }
            else
            {
                activePerks.Add(perk, 1);
                Debug.Log($"[UpgradeManager] Đã nhận Perk mới: '{perk.PerkName}'");
            }

            OnPerkAdded?.Invoke(perk, activePerks[perk]);

            // Áp dụng ngay hiệu ứng lên Player trong Scene
            ApplyPerksToCurrentPlayer();
        }

        /// <summary>
        /// Giảm stack hoặc xóa hoàn toàn một Perk khỏi danh sách active.
        /// </summary>
        public void RemovePerk(PerkData perk)
        {
            if (perk == null) return;

            if (activePerks.ContainsKey(perk))
            {
                if (activePerks[perk] > 1)
                {
                    activePerks[perk]--;
                    Debug.Log($"[UpgradeManager] Giảm 1 stack Perk '{perk.PerkName}': {activePerks[perk]}/{perk.MaxStack}");
                }
                else
                {
                    activePerks.Remove(perk);
                    Debug.Log($"[UpgradeManager] Đã xóa hoàn toàn Perk '{perk.PerkName}'");
                }

                // Áp dụng lại để cập nhật chỉ số Player
                ApplyPerksToCurrentPlayer();
            }
        }


        /// <summary>
        /// Áp dụng lại toàn bộ active perks lên Player hiện tại.
        /// </summary>
        public void ApplyPerksToCurrentPlayer()
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                PerkEffectApplier.ApplyAllActivePerks(playerStats.gameObject, activePerks);
            }
        }

        /// <summary>
        /// Kiểm tra xem người chơi có sở hữu một hành vi đặc biệt nào không và lấy số stack.
        /// </summary>
        public bool HasSpecialBehavior(string behaviorKey, out int stackCount)
        {
            stackCount = 0;
            foreach (var kvp in activePerks)
            {
                if (kvp.Key.EffectType == PerkEffectType.SpecialBehavior && kvp.Key.SpecialBehaviorKey == behaviorKey)
                {
                    stackCount = kvp.Value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Rút ngẫu nhiên một danh sách Perk độc nhất dựa theo trọng số độ hiếm.
        /// Loại trừ các Perk đã đạt max stack.
        /// </summary>
        public List<PerkData> GetRandomPerks(int count)
        {
            List<PerkData> result = new List<PerkData>();
            if (perkPool == null || perkPool.AllPerks.Count == 0)
            {
                Debug.LogError("[UpgradeManager] PerkPool trống hoặc chưa được gán!");
                return result;
            }

            // 1. Tạo danh sách ứng viên hợp lệ (chưa đạt max stack)
            List<PerkData> candidates = new List<PerkData>();
            foreach (PerkData perk in perkPool.AllPerks)
            {
                if (perk == null) continue;

                bool isMaxStacked = activePerks.ContainsKey(perk) && activePerks[perk] >= perk.MaxStack;
                if (!isMaxStacked)
                {
                    candidates.Add(perk);
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[UpgradeManager] Không còn Perk hợp lệ nào trong pool!");
                return result;
            }

            // 2. Rút ngẫu nhiên không trùng lặp theo trọng số
            int itemsToGet = Mathf.Min(count, candidates.Count);
            for (int i = 0; i < itemsToGet; i++)
            {
                PerkData chosen = PickWeightedPerk(candidates);
                if (chosen != null)
                {
                    result.Add(chosen);
                    candidates.Remove(chosen); // Tránh rút trùng lặp
                }
            }

            return result;
        }

        /// <summary>
        /// Thuật toán weighted random chọn 1 Perk từ danh sách candidates.
        /// </summary>
        private PerkData PickWeightedPerk(List<PerkData> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            // Tính tổng trọng số
            int totalWeight = 0;
            foreach (PerkData perk in candidates)
            {
                totalWeight += GetRarityWeight(perk.Rarity);
            }

            if (totalWeight <= 0) return candidates[UnityEngine.Random.Range(0, candidates.Count)];

            // Rút số ngẫu nhiên
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int currentSum = 0;

            foreach (PerkData perk in candidates)
            {
                currentSum += GetRarityWeight(perk.Rarity);
                if (roll < currentSum)
                {
                    return perk;
                }
            }

            return candidates[candidates.Count - 1];
        }

        /// <summary>
        /// Lấy giá trị trọng số tương ứng với độ hiếm.
        /// </summary>
        private int GetRarityWeight(PerkRarity rarity)
        {
            switch (rarity)
            {
                case PerkRarity.Common: return commonWeight;
                case PerkRarity.Rare: return rareWeight;
                case PerkRarity.Epic: return epicWeight;
                case PerkRarity.Legendary: return legendaryWeight;
                default: return 0;
            }
        }

        /// <summary>
        /// Lưu lịch sử các Perk nhận được trong lượt chơi vừa qua.
        /// </summary>
        private void SaveRunHistory()
        {
            lastRunHistory.Clear();
            foreach (var kvp in activePerks)
            {
                // Thêm bao nhiêu lần tương đương với số stack
                for (int i = 0; i < kvp.Value; i++)
                {
                    lastRunHistory.Add(kvp.Key);
                }
            }
            Debug.Log($"[UpgradeManager] Đã lưu lịch sử run vừa qua với {lastRunHistory.Count} Perks.");
        }

        /// <summary>
        /// Dọn dẹp danh sách active perks để sẵn sàng cho run mới.
        /// </summary>
        public void ClearActivePerks()
        {
            activePerks.Clear();
            Debug.Log("[UpgradeManager] Đã dọn dẹp sạch sẽ danh sách Active Perks.");
            ApplyPerksToCurrentPlayer(); // Reset chỉ số Player về mặc định
        }

        // --- LISTENERS ---

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Khi load scene gameplay, tự động áp dụng lại Perks lên Player mới sinh ra
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log($"[UpgradeManager] Phát hiện Player trong scene mới '{scene.name}'. Áp dụng lại active perks...");
                ApplyPerksToCurrentPlayer();
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            // Khi thua cuộc hoặc thắng cuộc (kết thúc run), lưu lịch sử và dọn dẹp
            if (state == GameState.GameOver || state == GameState.Victory)
            {
                SaveRunHistory();
                ClearActivePerks();
            }
        }

        private void OnEnemyDied(EnemyBase enemy)
        {
            // Xử lý Special Behavior khi quái chết
            if (HasSpecialBehavior("heal_on_kill", out int stackCount))
            {
                PlayerStats playerStats = FindObjectOfType<PlayerStats>();
                if (playerStats != null && !playerStats.IsDead)
                {
                    // Lấy Perk heal_on_kill để đọc giá trị hồi máu cấu hình (hoặc fallback = 5)
                    float healAmount = 5f;
                    foreach (var kvp in activePerks)
                    {
                        if (kvp.Key.SpecialBehaviorKey == "heal_on_kill")
                        {
                            healAmount = kvp.Key.EffectValue;
                            break;
                        }
                    }

                    float totalHeal = healAmount * stackCount;
                    playerStats.Heal(totalHeal);
                    Debug.Log($"[UpgradeManager] Kích hoạt 'heal_on_kill': Hồi {totalHeal} HP cho Player.");
                }
            }
        }
    }
}
