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
    }
}