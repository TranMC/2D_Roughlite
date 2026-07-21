using UnityEngine;

namespace Roguelite.Enemy
{
    /// <summary>
    /// Boss demo dùng để test tích hợp hệ thống Phase + mở cửa phòng Boss.
    /// Gắn vào Boss GameObject trong Prefab Boss Room, chuột phải Inspector để test.
    /// </summary>
    public class Boss: BossBase
    {
        private Material originalMaterial;
        
        /// <summary>Scale gốc lúc Start, dùng làm mốc tính Enrage scale.</summary>
        private Vector3 originalScale;

        // =====================================================================
        //  LIFECYCLE
        // =====================================================================

        protected override void Start()
        {
            base.Start();

            originalScale = transform.localScale;

            if (spriteRenderer != null)
            {
                originalMaterial = spriteRenderer.material;
            }

            OnPhaseChanged += HandlePhaseChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnPhaseChanged -= HandlePhaseChanged;
            
            // Restore original material khi destroy
            if (spriteRenderer != null && originalMaterial != null)
            {
                spriteRenderer.material = originalMaterial;
            }
        }

        // =====================================================================
        //  PHASE CHANGED – Apply Material Outline
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
                        
                        // Restore original material khi boss chết
                        if (spriteRenderer != null && originalMaterial != null)
                        {
                            spriteRenderer.material = originalMaterial;
                        }
                        
                        Debug.Log("[DemoBoss] Boss chết: restore original material");
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
            
            if (newPhase == 0)
            {
                // Restore original material khi về phase 0
                if (originalMaterial != null)
                {
                    spriteRenderer.material = originalMaterial;
                }
            }
            else
            {
                // Apply material từ PhasePatternGroup tương ứng
                int groupIndex = Mathf.Clamp(newPhase, 0, phasePatterns.Count - 1);
                if (phasePatterns != null && groupIndex < phasePatterns.Count && phasePatterns[groupIndex] != null)
                {
                    Material phaseMaterial = phasePatterns[groupIndex].enragedMaterial;
                    if (phaseMaterial != null)
                    {
                        spriteRenderer.material = phaseMaterial;
                    }
                    else if (originalMaterial != null)
                    {
                        // Fallback về original material nếu phase không có material
                        spriteRenderer.material = originalMaterial;
                    }
                }
                else if (originalMaterial != null)
                {
                    // Fallback về original material nếu không có phase pattern
                    spriteRenderer.material = originalMaterial;
                }
            }
        
            Debug.Log($"[DemoBoss] Phase {newPhase}: {(newPhase == 0 ? "restored original material" : "applied phase material")}");
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
