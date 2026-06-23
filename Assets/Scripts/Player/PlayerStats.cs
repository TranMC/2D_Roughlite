using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Roguelite.Combat;

namespace Roguelite.Player
{
    /// <summary>
    /// Quản lý lượng máu (HP), các sự kiện Hit, Dead và các trạng thái như bất tử tạm thời (invincibility), khóa vận tốc (lock velocity) của người chơi.
    /// </summary>
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        // === Debug Config ===
        private const string MODULE_NAME = "PlayerStats";

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebug = true;
        [SerializeField] private bool logDamage = true;
        [SerializeField] private bool logHealthChanges = true;
        [SerializeField] private bool logDeath = true;
        [SerializeField] private bool logInvincibility = false;

        [Header("Health Settings")]
        [Tooltip("Lượng máu tối đa của nhân vật.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Lượng máu hiện tại của nhân vật.")]
        [SerializeField] private float currentHealth;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;

        // Sự kiện xảy ra khi máu thay đổi (lượng máu hiện tại, lượng máu tối đa)
        public event Action<float, float> OnHealthChanged;
        
        // Sự kiện khi nhân vật nhận sát thương
        public event Action OnHit;
        
        // Sự kiện khi nhân vật hết máu
        public event Action OnDead;

        [Header("Unity Events (for inspector/UI)")]
        public UnityEvent<float, float> healthChanged;
        public UnityEvent<float, Vector2> damageableHit;

        [Header("Status Settings")]
        [Tooltip("Trạng thái sinh tử của nhân vật.")]
        [SerializeField] private bool isDead = false;

        [Header("Invincibility Settings")]
        [SerializeField] private bool isInvincible = false;
        [SerializeField] private float invincibilityTimer = 0.25f;
        private float timeSinceHit = 0f;

        public bool IsDead => isDead;

        public bool LockVelocity
        {
            get
            {
                if (playerController != null && playerController.Animator != null)
                {
                    return playerController.Animator.GetBool(AnimationStrings.lockVelocity);
                }
                return false;
            }
            set
            {
                if (playerController != null && playerController.Animator != null)
                {
                    playerController.Animator.SetBool(AnimationStrings.lockVelocity, value);
                }
            }
        }

        private PlayerController playerController;

        private void Awake()
        {
            // Lấy tham chiếu tới bộ điều khiển nhân vật chính
            playerController = GetComponent<PlayerController>();

            // Đăng ký module debug
            DebugLogger.SetModuleDebug(MODULE_NAME, enableDebug);
            DebugLogger.Log($"PlayerStats initialized on '{gameObject.name}'", MODULE_NAME, DebugLogger.LogType.Success);
        }

        private void Start()
        {
            // Thiết lập lượng máu ban đầu bằng máu tối đa
            currentHealth = maxHealth;
            isDead = false;
            isInvincible = false;
            timeSinceHit = 0f;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            healthChanged?.Invoke(currentHealth, maxHealth);

            if (playerController != null && playerController.Animator != null)
            {
                playerController.Animator.SetBool(AnimationStrings.isAlive, true);
                playerController.Animator.SetBool(AnimationStrings.isDead, false);
            }
            LockVelocity = false; // Đảm bảo mở khóa di chuyển khi bắt đầu game
        }

        private void Update()
        {
            // Xử lý đếm ngược thời gian bất tử
            if (isInvincible)
            {
                if (timeSinceHit > invincibilityTimer)
                {
                    isInvincible = false;
                    timeSinceHit = 0f;
                    LockVelocity = false; // Mở khóa di chuyển cho Player sau khi hết bất tử/stagger

                    if (logInvincibility)
                    {
                        DebugLogger.Log("Invincibility ended", MODULE_NAME);
                    }
                }

                timeSinceHit += Time.deltaTime;
            }

#if UNITY_EDITOR
            // Nhấn phím T để nhận 10 sát thương (Test Hit)
            if (Input.GetKeyDown(KeyCode.T))
            {
                TakeDamage(10f);
            }

            // Nhấn phím Y để nhận 100 sát thương (Test Die)
            if (Input.GetKeyDown(KeyCode.Y))
            {
                TakeDamage(100f);
            }

            // Nhấn phím U để hồi 10 máu (Test Heal)
            if (Input.GetKeyDown(KeyCode.U))
            {
                Heal(10f);
            }
#endif
        }

        /// <summary>
        /// Gây sát thương lên nhân vật chính.
        /// </summary>
        /// <param name="damage">Lượng sát thương nhận vào</param>
        public void TakeDamage(float damage)
        {
            TakeDamage(damage, Vector2.zero);
        }

        /// <summary>
        /// Gây sát thương lên nhân vật chính kèm theo lực đẩy (Knockback).
        /// </summary>
        /// <param name="damage">Lượng sát thương nhận vào</param>
        /// <param name="knockback">Lực đẩy áp dụng</param>
        public void TakeDamage(float damage, Vector2 knockback)
        {
            if (isDead || damage <= 0f || isInvincible)
            {
                if (isInvincible && logDamage && !isDead)
                {
                    DebugLogger.Log($"{gameObject.name} is invincible, damage blocked", MODULE_NAME);
                }
                return;
            }

            if (logDamage)
            {
                DebugLogger.LogWarning($"{gameObject.name} took {damage} damage! HP: {currentHealth} → {currentHealth - damage}", MODULE_NAME);
            }

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            isInvincible = true;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            healthChanged?.Invoke(currentHealth, maxHealth);

            // Áp dụng lực đẩy (Knockback) nếu có
            if (playerController != null && knockback != Vector2.zero)
            {
                if (playerController.Rb != null)
                {
                    playerController.Rb.velocity = new Vector2(knockback.x, playerController.Rb.velocity.y + knockback.y);
                }
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
            else
            {
                OnHit?.Invoke();
                damageableHit?.Invoke(damage, knockback);
                
                // Kích hoạt Animator trigger hit nếu có
                if (playerController != null && playerController.Animator != null)
                {
                    playerController.Animator.SetTrigger(AnimationStrings.hitTrigger);
                }
                LockVelocity = true;

                if (logInvincibility)
                {
                    DebugLogger.Log($"Invincibility activated for {invincibilityTimer}s", MODULE_NAME, DebugLogger.LogType.Info);
                }
            }
        }

        /// <summary>
        /// Hồi máu cho nhân vật. Trả về true nếu hồi máu thành công, false nếu không.
        /// </summary>
        /// <param name="amount">Lượng máu hồi phục</param>
        public bool Heal(float amount)
        {
            if (isDead || amount <= 0f || currentHealth >= maxHealth)
            {
                if (logHealthChanges && currentHealth >= maxHealth)
                {
                    DebugLogger.LogWarning("Already at max health, cannot heal", MODULE_NAME);
                }
                return false;
            }

            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            float actualHealed = currentHealth - oldHealth;

            if (logHealthChanges)
            {
                DebugLogger.Log($"Healed {actualHealed} HP! {oldHealth} → {currentHealth}", MODULE_NAME, DebugLogger.LogType.Success);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            healthChanged?.Invoke(currentHealth, maxHealth);
            return true;
        }

        /// <summary>
        /// Xử lý logic khi nhân vật hết máu và chết.
        /// </summary>
        private void Die()
        {
            isDead = true;
            OnDead?.Invoke();

            if (logDeath)
            {
                DebugLogger.Log($"{gameObject.name} DIED!", MODULE_NAME, DebugLogger.LogType.Error);
            }

            if (playerController != null)
            {
                // Vô hiệu hóa script điều khiển PlayerController để dừng thu thập input
                playerController.SetVelocityX(0f);
                playerController.enabled = false;

                // Kiểm tra trạng thái chạm đất thực tế trước khi tắt TouchingDirections
                TouchingDirections touchingDirections = GetComponent<TouchingDirections>();
                bool wasGrounded = touchingDirections != null && touchingDirections.IsGrounded;

                if (touchingDirections != null)
                {
                    touchingDirections.enabled = false;
                }

                // Xử lý mô phỏng vật lý: Cho phép rơi tự do nếu đang ở trên không
                if (playerController.Rb != null)
                {
                    if (wasGrounded)
                    {
                        playerController.Rb.velocity = Vector2.zero;
                        playerController.Rb.simulated = false;

                        Collider2D col = GetComponent<Collider2D>();
                        if (col != null)
                        {
                            col.enabled = false;
                        }
                    }
                    else
                    {
                        // Chỉ khóa di chuyển ngang để rơi tự do theo trục đứng
                        playerController.Rb.velocity = new Vector2(0f, playerController.Rb.velocity.y);
                        StartCoroutine(HandlePhysicsAfterDeath());
                    }
                }

                // Kích hoạt Animator trigger die và đặt bool isDead thành true để khóa trạng thái
                if (playerController.Animator != null)
                {
                    playerController.Animator.SetTrigger(AnimationStrings.dieTrigger);
                    playerController.Animator.SetBool(AnimationStrings.isDead, true);
                    playerController.Animator.SetBool(AnimationStrings.isAlive, false);

                    // Reset các tham số di chuyển/rơi để tránh Animator tự động quay về trạng thái nhảy/rơi từ Any State
                    playerController.Animator.SetBool(AnimationStrings.isGrounded, true);
                    playerController.Animator.SetFloat(AnimationStrings.yVelocity, 0f);
                    playerController.Animator.SetBool(AnimationStrings.isMoving, false);
                    playerController.Animator.SetBool(AnimationStrings.isRunning, false);
                    playerController.Animator.SetBool(AnimationStrings.isJumping, false);
                }
            }

            // Gọi GameManager chuyển trạng thái sang GameOver
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeState(Core.GameState.GameOver);
            }
        }

        /// <summary>
        /// Coroutine quản lý vật lý của người chơi sau khi chết trên không:
        /// Cho phép người chơi rơi xuống đất rồi mới tắt mô phỏng vật lý.
        /// </summary>
        private IEnumerator HandlePhysicsAfterDeath()
        {
            // Chờ một khoảng thời gian ngắn để nhân vật bắt đầu rơi (tránh việc đứng yên lúc đầu có velocity.y = 0)
            yield return new WaitForSeconds(0.15f);

            // Chờ đến khi vận tốc rơi theo trục Y xấp xỉ bằng 0 (đã chạm đất)
            yield return new WaitUntil(() => playerController == null || playerController.Rb == null || Mathf.Abs(playerController.Rb.velocity.y) < 0.1f);

            if (playerController != null && playerController.Rb != null)
            {
                playerController.Rb.velocity = Vector2.zero;
                playerController.Rb.simulated = false;
            }

            // Vô hiệu hóa Collider để các đối tượng khác có thể đi xuyên qua xác Player
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        // --- CÁC HÀM HỖ TRỢ TEST TỪ INSPECTOR (RIGHT-CLICK) ---
        [ContextMenu("Test/Take 10 Damage")]
        private void TestTake10Damage() => TakeDamage(10f);

        [ContextMenu("Test/Take 100 Damage (Die)")]
        private void TestTake100Damage() => TakeDamage(100f);

        [ContextMenu("Test/Heal 10 HP")]
        private void TestHeal10HP() => Heal(10f);

        public void SetDebugEnabled(bool enabled)
        {
            enableDebug = enabled;
            DebugLogger.SetModuleDebug(MODULE_NAME, enabled);
        }
    }
}
