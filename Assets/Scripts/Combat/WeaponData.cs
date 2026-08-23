using UnityEngine;

namespace Roguelite.Combat
{
    /// <summary>
    /// ScriptableObject lưu trữ thông số cơ bản của một vũ khí.
    /// Được truyền qua event weaponSwitched khi Player đổi vũ khí.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Roguelite/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [SerializeField] private string weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private Sprite icon;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Chỉ số chiến đấu")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float range = 1.5f;
        [SerializeField] private Vector2 knockback = new Vector2(3f, 0f);

        [Header("Âm thanh (SFX)")]
        [SerializeField] private AudioClip shootSFX;
        [SerializeField] private AudioClip hitSFX;

        [Header("Hitbox Data theo Animation State")]
        [Tooltip("Gán bộ HitboxData riêng cho vũ khí này. Nếu để trống, dùng HitboxData mặc định trên Player.")]
        [SerializeField] private WeaponHitboxMapping[] hitboxMappings;

        [System.Serializable]
        public struct WeaponHitboxMapping
        {
            [Tooltip("Tên animation state (vd: Attack1, Attack2, Attack3, AirAttack)")]
            public string animationStateName;
            public HitboxData hitboxData;
        }

        // === Properties (read-only) ===
        public string WeaponId => weaponId;
        public string WeaponName => weaponName;
        public Sprite Icon => icon;
        public string Description => description;
        public float Damage => damage;
        public float AttackSpeed => attackSpeed;
        public float Range => range;
        public Vector2 Knockback => knockback;
        public AudioClip ShootSFX => shootSFX;
        public AudioClip HitSFX => hitSFX;
        public WeaponHitboxMapping[] HitboxMappings => hitboxMappings;

        /// <summary>Kiểm tra vũ khí này có bộ hitbox mapping riêng hay không.</summary>
        public bool HasHitboxMappings => hitboxMappings != null && hitboxMappings.Length > 0;
    }
}