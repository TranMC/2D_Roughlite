using System;
using UnityEngine;
using Roguelite.Combat;

namespace Roguelite.Player
{
    /// <summary>
    /// Quản lý lượng máu (HP) và các sự kiện Hit, Dead của người chơi.
    /// </summary>
    public class PlayerStats : MonoBehaviour, IDamageable
    {
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

        [Header("Status Settings")]
        [Tooltip("Trạng thái sinh tử của nhân vật.")]
        [SerializeField] private bool isDead = false;

        public bool IsDead => isDead;
        private PlayerController playerController;

        private void Awake()
        {
            // Lấy tham chiếu tới bộ điều khiển nhân vật chính
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            // Thiết lập lượng máu ban đầu bằng máu tối đa
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
            if (isDead || damage <= 0f) return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            Debug.Log($"[PlayerStats] Nhân vật nhận {damage} sát thương. Máu hiện tại: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Áp dụng lực đẩy (Knockback) nếu có
            if (playerController != null && knockback != Vector2.zero)
            {
                if (playerController.Rb != null)
                {
                    // Đặt vận tốc tức thời hoặc dùng AddForce (Có thể tinh chỉnh thêm trong trạng thái bị trúng đòn)
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
                
                // Kích hoạt Animator trigger hit nếu có
                if (playerController != null && playerController.Animator != null)
                {
                    playerController.Animator.SetTrigger(AnimationStrings.hitTrigger);
                }
            }
        }

        /// <summary>
        /// Hồi máu cho nhân vật.
        /// </summary>
        /// <param name="amount">Lượng máu hồi phục</param>
        public void Heal(float amount)
        {
            if (isDead || amount <= 0f) return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            Debug.Log($"[PlayerStats] Nhân vật được hồi {amount} HP. Máu hiện tại: {currentHealth}/{maxHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Xử lý logic khi nhân vật hết máu và chết.
        /// </summary>
        private void Die()
        {
            isDead = true;
            OnDead?.Invoke();

            Debug.Log("[PlayerStats] Nhân vật đã chết!");

            if (playerController != null)
            {
                // Vô hiệu hóa script điều khiển PlayerController để dừng thu thập input
                playerController.SetVelocityX(0f);
                playerController.enabled = false;

                // Dừng mô phỏng vật lý của Player nếu có Rigidbody2D
                if (playerController.Rb != null)
                {
                    playerController.Rb.velocity = Vector2.zero;
                    playerController.Rb.simulated = false;
                }

                // Kích hoạt Animator trigger die và đặt bool isDead thành true để khóa trạng thái
                if (playerController.Animator != null)
                {
                    playerController.Animator.SetTrigger(AnimationStrings.dieTrigger);
                    playerController.Animator.SetBool(AnimationStrings.isDead, true);
                }
            }

            // Gọi GameManager chuyển trạng thái sang GameOver
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeState(Core.GameState.GameOver);
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
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
        }
#endif

        // --- CÁC HÀM HỖ TRỢ TEST TỪ INSPECTOR (RIGHT-CLICK) ---
        [ContextMenu("Test/Take 10 Damage")]
        private void TestTake10Damage() => TakeDamage(10f);

        [ContextMenu("Test/Take 100 Damage (Die)")]
        private void TestTake100Damage() => TakeDamage(100f);

        [ContextMenu("Test/Heal 10 HP")]
        private void TestHeal10HP() => Heal(10f);
    }
}
