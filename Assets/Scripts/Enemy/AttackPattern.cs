using UnityEngine;

namespace Roguelite.Enemy
{
    [CreateAssetMenu(fileName = "NewAttackPattern", menuName = "Roguelite/Boss/Attack Pattern")]
    public class AttackPattern : ScriptableObject
    {
        [Header("General Info")]
        [Tooltip("Tên của attack pattern này.")]
        [SerializeField] private string patternName;

        [Tooltip("Trigger animation tương ứng trong Animator.")]
        [SerializeField] private string animationTrigger;

        [Tooltip("Thời gian khóa di chuyển của Boss khi ra chiêu (giây).")]
        [SerializeField] private float attackLockDuration = 1.0f;

        [Header("Hitbox Timing Settings")]
        [Tooltip("Nếu true, hitbox sẽ bật/tắt bằng Animation Event từ clip. Nếu false, sử dụng thời gian timer bên dưới.")]
        [SerializeField] private bool useAnimationEvents = false;

        [Tooltip("Thời gian delay trước khi kích hoạt hitbox (chỉ dùng nếu useAnimationEvents = false).")]
        [SerializeField] private float startDelay = 0.3f;

        [Tooltip("Thời gian hitbox hoạt động (chỉ dùng nếu useAnimationEvents = false).")]
        [SerializeField] private float activeDuration = 0.2f;

        [Header("Hitbox Shape & Position")]
        [Tooltip("Nếu true, sử dụng CircleCollider2D. Nếu false, sử dụng BoxCollider2D.")]
        [SerializeField] private bool isCircle = false;

        [Tooltip("Vị trí tương đối của hitbox so với Boss.")]
        [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

        [Tooltip("Kích thước của hitbox (Box: Width/Height, Circle: X đại diện cho Radius).")]
        [SerializeField] private Vector2 hitboxSize = Vector2.one;

        [Header("Damage Settings")]
        [Tooltip("Sát thương gây ra bởi pattern này.")]
        [SerializeField] private float damage = 15f;

        [Tooltip("Lực đẩy lùi (Knockback) của đòn đánh.")]
        [SerializeField] private Vector2 knockback = Vector2.zero;

        // --- PROPERTIES (GETTERS) ---
        public string PatternName => patternName;
        public string AnimationTrigger => animationTrigger;
        public float AttackLockDuration => attackLockDuration;
        public bool UseAnimationEvents => useAnimationEvents;
        public float StartDelay => startDelay;
        public float ActiveDuration => activeDuration;
        public bool IsCircle => isCircle;
        public Vector2 HitboxOffset => hitboxOffset;
        public Vector2 HitboxSize => hitboxSize;
        public float Damage => damage;
        public Vector2 Knockback => knockback;
    }
}
