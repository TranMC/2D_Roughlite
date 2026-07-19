using UnityEngine;

namespace Roguelite.Combat
{
    [CreateAssetMenu(fileName = "NewAttackConfig", menuName = "Roguelite/Combat/Attack Config")]
    public class AttackConfig : ScriptableObject
    {
        [Header("Timing (seconds)")]
        [SerializeField] private float activateTime = 0.167f;
        [SerializeField] private float deactivateTime = 0.3f;

        [Header("Hitbox Shape & Position")]
        [SerializeField] private bool isCircle = false;
        [SerializeField] private Vector2 hitboxSize = Vector2.one;
        [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

        [Header("Damage")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private Vector2 knockback = Vector2.zero;

        public float ActivateTime => activateTime;
        public float DeactivateTime => deactivateTime;
        public bool IsCircle => isCircle;
        public Vector2 HitboxSize => hitboxSize;
        public Vector2 HitboxOffset => hitboxOffset;
        public float Damage => damage;
        public Vector2 Knockback => knockback;

        public void SetTiming(float activate, float deactivate)
        {
            activateTime = activate;
            deactivateTime = deactivate;
        }

        public void SetHitboxShape(bool circle, Vector2 size, Vector2 offset)
        {
            isCircle = circle;
            hitboxSize = size;
            hitboxOffset = offset;
        }

        public void SetDamage(float dmg, Vector2 knock)
        {
            damage = dmg;
            knockback = knock;
        }
    }
}
