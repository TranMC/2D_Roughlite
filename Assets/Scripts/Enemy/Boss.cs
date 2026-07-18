using UnityEngine;

namespace Roguelite.Enemy
{
    /// <summary>
    /// Boss demo dùng để test tích hợp hệ thống Phase + mở cửa phòng Boss.
    /// Gắn vào Boss GameObject trong Prefab Boss Room, chuột phải Inspector để test.
    /// </summary>
    public class Boss: BossBase
    {
        /// <summary>Scale gốc lúc Start, dùng làm mốc tính Enrage scale.</summary>
        private Vector3 originalScale;

        /// <summary>Màu gốc của SpriteRenderer.</summary>
        private Color originalColor;

        // =====================================================================
        //  LIFECYCLE
        // =====================================================================

        protected override void Start()
        {
            base.Start();

            originalScale = transform.localScale;

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            OnPhaseChanged += HandlePhaseChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnPhaseChanged -= HandlePhaseChanged;
        }

        // =====================================================================
        //  PHASE CHANGED – Đổi màu
        // =====================================================================

        protected override void OnStateEnter(EnemyState enteringState, EnemyState previousState)
        {
            base.OnStateEnter(enteringState, previousState);
            
            // Kích hoạt Animation giống như Enemy_AI
            if (anim != null)
            {
                switch (enteringState)
                {
                    case EnemyState.Attack:
                        if (!IsAttackingPattern)
                        {
                            anim.SetTrigger("AI_attack");
                        }
                        break;
                    case EnemyState.Dead:
                        anim.SetTrigger("AI_die");
                        break;
                    case EnemyState.Hit:
                        anim.SetTrigger("AI_hit");
                        break;
                }
            }
        }

        private void HandlePhaseChanged(int newPhase)
        {
            if (spriteRenderer == null) return;

            switch (newPhase)
            {
                case 1:
                    spriteRenderer.color = Color.yellow;
                    break;
                default: // Phase >= 2
                    spriteRenderer.color = Color.red;
                    break;
            }

            Debug.Log($"[DemoBoss] Phase {newPhase}: màu={spriteRenderer.color}");
        }

        // =====================================================================
        //  CONTEXT MENU – Test nhanh từ Inspector (chuột phải)
        // =====================================================================

        [ContextMenu("Debug/Gây 20% maxHP sát thương")]
        private void DebugTakeDamage()
        {
            float damage = maxHP * 0.2f;
            TakeDamage(damage);
            Debug.Log($"[DemoBoss] Debug: gây {damage} sát thương ({currentHP}/{maxHP} HP còn lại)");
        }

        [ContextMenu("Debug/Hạ gục Boss ngay lập tức")]
        private void DebugKillBoss()
        {
            TakeDamage(currentHP + 1f);
            Debug.Log("[DemoBoss] Debug: Boss bị hạ gục!");
        }
    }
}
