using UnityEngine;

namespace Roguelite.Combat
{
    /// <summary>
    /// Xử lý việc gây sát thương và lực đẩy (Knockback) thông qua Trigger Collider va chạm.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Attack : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool logAttackHits = true;

        [Header("Attack Settings")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private Vector2 knockback = Vector2.zero;

        private float baseAttackDamage;
        private Collider2D attackCollider;

        public float AttackDamage
        {
            get => attackDamage;
            set => attackDamage = value;
        }

        public Vector2 Knockback
        {
            get => knockback;
            set => knockback = value;
        }

        private void Awake()
        {
            attackCollider = GetComponent<Collider2D>();
            // Đảm bảo Collider là trigger để phát hiện va chạm không cản trở vật lý
            attackCollider.isTrigger = true;

            // Lưu trữ sát thương cơ bản ban đầu
            baseAttackDamage = attackDamage;
        }

        /// <summary>
        /// Áp dụng bổ trợ để thay đổi sát thương đòn đánh động.
        /// </summary>
        public void ApplyDamageModifier(float flatBonus, float percentBonus)
        {
            attackDamage = (baseAttackDamage + flatBonus) * (1f + percentBonus);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Kiểm tra xem đối tượng va chạm có thể nhận sát thương không
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Xác định hướng knockback dựa trên localScale.x của cha (hướng mặt của nhân vật tấn công)
                Transform parentTransform = transform.parent;
                float faceDirection = parentTransform != null ? parentTransform.localScale.x : 1f;
                Vector2 deliveredKnockback = faceDirection > 0f ? knockback : new Vector2(-knockback.x, knockback.y);

                // Thực hiện gây sát thương và áp dụng knockback
                damageable.TakeDamage(attackDamage, deliveredKnockback);

                if (logAttackHits)
                {
                    string attackerName = parentTransform != null ? parentTransform.name : gameObject.name;
                    Debug.Log($"[Attack] {attackerName} va chạm gây sát thương lên {collision.name}: {attackDamage} dmg, Knockback: {deliveredKnockback}");
                }
            }
        }
    }
}
