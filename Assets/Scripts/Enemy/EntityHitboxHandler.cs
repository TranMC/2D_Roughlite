using System.Collections;
using UnityEngine;
using Roguelite.Combat;

namespace Roguelite.Enemy
{
    public class EntityHitboxHandler : MonoBehaviour
    {
        [Header("Collider References")]
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private CircleCollider2D circleCollider;

        [Header("Combat Script")]
        [SerializeField] private Attack attackComponent;

        private Coroutine activeAttackCoroutine;
        private AttackPattern currentPattern;

        private void Awake()
        {
            // Auto-assign references if not set in Inspector
            if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
            if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
            if (attackComponent == null) attackComponent = GetComponent<Attack>();

            // Ensure colliders start disabled
            DisableHitboxes();
        }

        /// <summary>
        /// Kích hoạt và cấu hình đòn đánh theo AttackPattern.
        /// Tự động lựa chọn giữa Animation Event hay Timer dựa trên cấu hình của pattern.
        /// </summary>
        public void ExecuteAttack(AttackPattern pattern)
        {
            currentPattern = pattern;

            // Dừng coroutine cũ nếu đang chạy
            if (activeAttackCoroutine != null)
            {
                StopCoroutine(activeAttackCoroutine);
                activeAttackCoroutine = null;
            }

            // Cấu hình các thông số của Attack component
            if (attackComponent != null)
            {
                attackComponent.AttackDamage = pattern.Damage;
                attackComponent.Knockback = pattern.Knockback;
            }

            // Nếu không dùng Animation Events, chạy Timer
            if (!pattern.UseAnimationEvents)
            {
                activeAttackCoroutine = StartCoroutine(TimerAttackCoroutine(pattern));
            }
        }

        /// <summary>
        /// Hủy bỏ/Dừng đòn đánh hiện tại ngay lập tức.
        /// </summary>
        public void StopAttack()
        {
            if (activeAttackCoroutine != null)
            {
                StopCoroutine(activeAttackCoroutine);
                activeAttackCoroutine = null;
            }
            DisableHitboxes();
        }

        // =====================================================================
        //  ANIMATION EVENT HANDLERS
        // =====================================================================

        /// <summary>
        /// Được gọi từ Animation Event để kích hoạt hitbox.
        /// </summary>
        public void StartAttackHitbox()
        {
            if (currentPattern == null || !currentPattern.UseAnimationEvents) return;
            EnableHitbox(currentPattern);
        }

        /// <summary>
        /// Được gọi từ Animation Event để tắt hitbox.
        /// </summary>
        public void EndAttackHitbox()
        {
            DisableHitboxes();
        }

        // =====================================================================
        //  HELPERS & COROUTINES
        // =====================================================================

        private IEnumerator TimerAttackCoroutine(AttackPattern pattern)
        {
            // 1. Chờ hết thời gian delay trước khi ra chiêu
            yield return new WaitForSeconds(pattern.StartDelay);

            // 2. Kích hoạt và định hình hitbox
            EnableHitbox(pattern);

            // 3. Chờ hết thời gian active của hitbox
            yield return new WaitForSeconds(pattern.ActiveDuration);

            // 4. Tắt hitbox
            DisableHitboxes();
            activeAttackCoroutine = null;
        }

        private void EnableHitbox(AttackPattern pattern)
        {
            if (pattern.IsCircle)
            {
                if (circleCollider != null)
                {
                    circleCollider.offset = pattern.HitboxOffset;
                    circleCollider.radius = pattern.HitboxSize.x;
                    circleCollider.enabled = true;
                }
                if (boxCollider != null) boxCollider.enabled = false;
            }
            else
            {
                if (boxCollider != null)
                {
                    boxCollider.offset = pattern.HitboxOffset;
                    boxCollider.size = pattern.HitboxSize;
                    boxCollider.enabled = true;
                }
                if (circleCollider != null) circleCollider.enabled = false;
            }
        }

        private void DisableHitboxes()
        {
            if (boxCollider != null) boxCollider.enabled = false;
            if (circleCollider != null) circleCollider.enabled = false;
        }
    }
}
