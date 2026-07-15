using UnityEngine;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Định nghĩa độ hiếm của Perk.
    /// </summary>
    public enum PerkRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Phân loại kiểu tác động của Perk.
    /// </summary>
    public enum PerkEffectType
    {
        StatModifier,   // Thay đổi trực tiếp các chỉ số cơ bản
        SpecialBehavior // Kích hoạt các hành vi hoặc hiệu ứng đặc biệt
    }

    /// <summary>
    /// Các chỉ số của người chơi có thể nâng cấp.
    /// </summary>
    public enum PlayerStatType
    {
        None,
        MaxHealth,
        WalkSpeed,
        RunSpeed,
        AttackDamage,
        JumpImpulse
    }

    /// <summary>
    /// Quy tắc cộng dồn giá trị hiệu ứng khi nhận nhiều stack cùng loại.
    /// </summary>
    public enum StackBehavior
    {
        None,           // Không cho phép stack hoặc không tăng giá trị
        Additive,       // Cộng dồn tuyến tính: (Giá trị * số stack)
        Multiplicative  // Nhân dồn tỷ lệ: (1 + Giá trị) ^ số stack - 1
    }

    /// <summary>
    /// ScriptableObject chứa cấu hình cho một Perk cụ thể trong game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPerkData", menuName = "Roguelite/Upgrade System/Perk Data")]
    public class PerkData : ScriptableObject
    {
        [Header("General Info")]
        [Tooltip("ID duy nhất để định danh Perk (không trùng lặp).")]
        [SerializeField] private string id;

        [Tooltip("Tên hiển thị của Perk trên giao diện người dùng.")]
        [SerializeField] private string perkName;

        [Tooltip("Mô tả chi tiết về hiệu ứng của Perk.")]
        [TextArea(3, 5)]
        [SerializeField] private string description;

        [Tooltip("Hình ảnh biểu tượng hiển thị cho Perk.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Độ hiếm của Perk (ảnh hưởng đến trọng số ngẫu nhiên).")]
        [SerializeField] private PerkRarity rarity = PerkRarity.Common;

        [Header("Effect Settings")]
        [Tooltip("Loại hiệu ứng tác động.")]
        [SerializeField] private PerkEffectType effectType = PerkEffectType.StatModifier;

        [Tooltip("Chỉ số của Player chịu tác động (chỉ dùng cho StatModifier).")]
        [SerializeField] private PlayerStatType statType = PlayerStatType.None;

        [Tooltip("Giá trị của hiệu ứng (ví dụ: 20 cho +20 HP, 0.15 cho +15% Speed).")]
        [SerializeField] private float effectValue;

        [Tooltip("Nếu tích chọn, giá trị hiệu ứng sẽ được tính theo phần trạng (%). Ngược lại là cộng thẳng.")]
        [SerializeField] private bool isPercent = false;

        [Header("Special Behavior Config")]
        [Tooltip("Định danh hành vi đặc biệt (chỉ dùng cho SpecialBehavior, ví dụ: 'heal_on_kill').")]
        [SerializeField] private string specialBehaviorKey;

        [Header("Stacking Rules")]
        [Tooltip("Số lượng stack tối đa mà người chơi có thể sở hữu cho Perk này.")]
        [Min(1)]
        [SerializeField] private int maxStack = 1;

        [Tooltip("Quy tắc cộng dồn giá trị khi tích lũy nhiều stack.")]
        [SerializeField] private StackBehavior stackBehavior = StackBehavior.None;

        // --- PROPERTIES (GETTERS) ---
        public string Id => id;
        public string PerkName => perkName;
        public string Description => description;
        public Sprite Icon => icon;
        public PerkRarity Rarity => rarity;
        public PerkEffectType EffectType => effectType;
        public PlayerStatType StatType => statType;
        public float EffectValue => effectValue;
        public bool IsPercent => isPercent;
        public string SpecialBehaviorKey => specialBehaviorKey;
        public int MaxStack => maxStack;
        public StackBehavior StackBehavior => stackBehavior;
    }
}
