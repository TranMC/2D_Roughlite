using UnityEngine;

namespace Roguelite.Enemy
{
    /// <summary>
    /// Boss demo dùng để test tích hợp hệ thống Phase + mở cửa phòng Boss.
    /// Gắn vào Boss GameObject trong Prefab Boss Room, chuột phải Inspector để test.
    /// </summary>
    public class Boss: BossBase
    {
        [SerializeField] private HealthBar healthBar;

        [Tooltip("Khoảng cách bổ sung phía trên sprite Boss (world units).")]
        [SerializeField] private float healthBarPadding = 0.5f;

        [Tooltip("Kích thước thanh máu trong world units (rộng x cao).")]
        [SerializeField] private Vector2 healthBarWorldSize = new Vector2(3f, 0.45f);

        private bool ownsHealthBarInstance;
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

            SetupHealthBar();

            // Đăng ký event để cập nhật health bar
            OnDamageTaken += UpdateHealthBar;
        }

        private void SetupHealthBar()
        {
            // Prefab gán trực tiếp trong Inspector chưa có instance trong scene → phải Instantiate.
            if (healthBar != null && !healthBar.gameObject.scene.IsValid())
            {
                healthBar = Instantiate(healthBar);
                healthBar.name = "BossHealthBar";
                ownsHealthBarInstance = true;
            }
            else if (healthBar == null)
            {
                healthBar = FindObjectOfType<HealthBar>(true);
            }

            if (healthBar == null)
            {
                Debug.LogWarning("[Boss] Không tìm thấy HealthBar. Gán prefab BossHealthBar vào field Health Bar trên Boss.");
                return;
            }

            healthBar.SetWorldDisplaySize(healthBarWorldSize);

            if (spriteRenderer != null)
            {
                healthBar.SetFollowTarget(transform, spriteRenderer, healthBarPadding);
            }
            else
            {
                healthBar.SetFollowTarget(transform, new Vector3(0f, healthBarPadding, 0f));
            }

            healthBar.SetMaxHealth(maxHP);
            healthBar.Show();
        }

        private void UpdateHealthBar(float damage, float remainingHP)
        {
            if (healthBar != null)
            {
                healthBar.SetHealth(remainingHP);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnPhaseChanged -= HandlePhaseChanged;
            OnDamageTaken -= UpdateHealthBar;

            if (ownsHealthBarInstance && healthBar != null)
            {
                Destroy(healthBar.gameObject);
            }
            
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
            
            // Ẩn health bar khi boss chết
            if (enteringState == EnemyState.Dead && healthBar != null)
            {
                healthBar.Hide();
            }

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
