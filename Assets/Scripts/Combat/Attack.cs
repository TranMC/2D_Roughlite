using UnityEngine;
using Roguelite.UpgradeSystem;
using Roguelite.Player;

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

                // --- HÚT MÁU (LIFESTEAL) ---
                // Nếu kẻ tấn công (parent của component này) có PlayerStats -> Player là người gây sát thương
                PlayerStats attackerStats = parentTransform != null ? parentTransform.GetComponent<PlayerStats>() : null;
                if (attackerStats != null && !attackerStats.IsDead)
                {
                    if (UpgradeManager.Instance != null && UpgradeManager.Instance.HasSpecialBehavior("lifesteal", out int lifestealStack))
                    {
                        float lifestealPercent = 0.05f; // Mặc định 5%
                        foreach (var kvp in UpgradeManager.Instance.ActivePerks)
                        {
                            if (kvp.Key.SpecialBehaviorKey == "lifesteal")
                            {
                                lifestealPercent = kvp.Key.EffectValue;
                                break;
                            }
                        }
                        float healAmount = attackDamage * lifestealPercent * lifestealStack;
                        attackerStats.Heal(healAmount);
                        Debug.Log($"[Lifesteal] Kích hoạt: Player gây {attackDamage} damage, hồi {healAmount} HP.");
                    }
                }

                // --- PHẢN SÁT THƯƠNG (THORNS) ---
                // Nếu nạn nhân bị trúng đòn (collision) có PlayerStats -> Player là người nhận sát thương
                PlayerStats victimStats = collision.GetComponent<PlayerStats>();
                if (victimStats != null && !victimStats.IsDead)
                {
                    if (UpgradeManager.Instance != null && UpgradeManager.Instance.HasSpecialBehavior("thorns", out int thornsStack))
                    {
                        float thornsPercent = 0.1f; // Mặc định phản 10%
                        foreach (var kvp in UpgradeManager.Instance.ActivePerks)
                        {
                            if (kvp.Key.SpecialBehaviorKey == "thorns")
                            {
                                thornsPercent = kvp.Key.EffectValue;
                                break;
                            }
                        }
                        float thornsDamage = attackDamage * thornsPercent * thornsStack;

                        // Gây sát thương ngược lại cho kẻ tấn công (phải có IDamageable)
                        IDamageable attackerDamageable = parentTransform != null ? parentTransform.GetComponent<IDamageable>() : null;
                        if (attackerDamageable != null)
                        {
                            attackerDamageable.TakeDamage(thornsDamage);
                            Debug.Log($"[Thorns] Kích hoạt: Player nhận {attackDamage} damage, phản lại {thornsDamage} damage lên {parentTransform.name}.");
                        }
                    }
                }
            }
        }
    }
}
