using System;
using System.Collections;
using UnityEngine;
using Roguelite.Combat;

namespace Roguelite.Enemy
{
    // =====================================================================
    //  Enum định nghĩa các trạng thái của quái vật
    // =====================================================================
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Hit,
        Dead
    }

    // =====================================================================
    //  EnemyBase – Lớp trừu tượng nền tảng cho mọi loại quái vật
    //
    //  Kiến trúc:
    //    • State Machine nội bộ điều phối 5 trạng thái (Patrol/Chase/Attack/Hit/Dead).
    //    • Sử dụng Rigidbody2D (đồng nhất với PlayerController).
    //    • Implement IDamageable để tương thích với hệ thống Attack.cs sẵn có.
    //    • Mọi hàm logic state đều là protected virtual → dễ override ở lớp con.
    // =====================================================================
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        #region ====== SERIALIZE FIELDS ======

        [Header("===== Health Settings =====")]
        [Tooltip("Lượng máu tối đa của quái vật.")]
        [SerializeField] protected float maxHP = 100f;

        [Tooltip("Lượng máu hiện tại (tự gán = maxHP lúc Start).")]
        [SerializeField] protected float currentHP;

        [Header("===== Movement Settings =====")]
        [Tooltip("Tốc độ di chuyển khi rượt đuổi Player.")]
        [SerializeField] protected float moveSpeed = 4f;

        [Tooltip("Tốc độ di chuyển khi tuần tra (thường chậm hơn moveSpeed).")]
        [SerializeField] protected float patrolSpeed = 2f;

        [Header("===== Detection & Attack Settings =====")]
        [Tooltip("Phạm vi phát hiện Player để chuyển từ Patrol sang Chase.")]
        [SerializeField] protected float detectionRange = 8f;

        [Tooltip("Khoảng cách đủ gần để thực hiện tấn công.")]
        [SerializeField] protected float attackRange = 1.5f;

        [Tooltip("Thời gian hồi giữa hai lần tấn công (giây).")]
        [SerializeField] protected float attackCooldown = 1f;

        [Tooltip("Sát thương mỗi đòn đánh.")]
        [SerializeField] protected float attackDamage = 10f;

        [Tooltip("Lực đẩy (Knockback) áp dụng lên Player khi đánh trúng.")]
        [SerializeField] protected Vector2 attackKnockback = new Vector2(5f, 2f);

        [Header("===== Hit Stagger Settings =====")]
        [Tooltip("Thời gian khựng lại khi bị trúng đòn (giây).")]
        [SerializeField] protected float hitStaggerDuration = 0.3f;

        [Header("===== Edge & Wall Detection =====")]
        [Tooltip("Điểm kiểm tra mép vực phía trước chân quái (gán trong Inspector).")]
        [SerializeField] protected Transform edgeCheckPoint;

        [Tooltip("Khoảng cách Raycast dò mặt đất phía trước (Edge Detection).")]
        [SerializeField] protected float edgeCheckDistance = 1.5f;

        [Tooltip("Khoảng cách Raycast dò tường phía trước (Wall Detection).")]
        [SerializeField] protected float wallCheckDistance = 0.5f;

        [Header("===== Layer Masks =====")]
        [Tooltip("Layer dùng để nhận diện Player (cho Detection và Attack).")]
        [SerializeField] protected LayerMask playerLayer;

        [Tooltip("Layer dùng để nhận diện mặt đất (cho Edge/Wall check).")]
        [SerializeField] protected LayerMask groundLayer;

        #endregion

        #region ====== CACHED COMPONENTS ======

        protected Rigidbody2D rb;
        protected Animator anim;
        protected SpriteRenderer spriteRenderer;

        #endregion

        #region ====== RUNTIME STATE ======

        /// <summary>Trạng thái hiện tại của quái vật.</summary>
        public EnemyState CurrentState { get; protected set; }

        /// <summary>Hướng quay mặt: 1 = phải, -1 = trái.</summary>
        protected int facingDirection = 1;

        /// <summary>Bộ đếm cooldown tấn công (đếm ngược về 0).</summary>
        protected float attackCooldownTimer;

        /// <summary>Tham chiếu tới Transform của Player (cache khi detect).</summary>
        protected Transform playerTarget;

        /// <summary>Coroutine đang xử lý trạng thái Hit (để dừng nếu bị hit liên tiếp).</summary>
        protected Coroutine hitCoroutine;

        /// <summary>Cờ khoá toàn bộ logic khi quái đã chết.</summary>
        protected bool isDead;

        #endregion

        #region ====== EVENTS ======

        /// <summary>Sự kiện khi quái nhận sát thương (lượng damage, HP còn lại).</summary>
        public event Action<float, float> OnDamageTaken;

        /// <summary>Sự kiện khi quái chết.</summary>
        public event Action OnDied;

        #endregion

        // =====================================================================
        //  UNITY LIFECYCLE
        // =====================================================================

        protected virtual void Awake()
        {
            // Cache các component cần thiết
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            currentHP = maxHP;
            isDead = false;
            attackCooldownTimer = 0f;

            // Bắt đầu ở trạng thái Tuần tra
            TransitionToState(EnemyState.Patrol);
        }

        protected virtual void Update()
        {
            if (isDead) return;

            // Giảm bộ đếm cooldown tấn công
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            // Điều phối logic theo State Machine
            switch (CurrentState)
            {
                case EnemyState.Patrol:
                    PatrolLogic();
                    break;
                case EnemyState.Chase:
                    ChaseLogic();
                    break;
                case EnemyState.Attack:
                    AttackLogic();
                    break;
                case EnemyState.Hit:
                    // Logic Hit được xử lý bởi Coroutine – không cần gọi liên tục
                    break;
                case EnemyState.Dead:
                    // Đã khóa toàn bộ logic
                    break;
            }
        }

        // =====================================================================
        //  STATE MACHINE – CHUYỂN TRẠNG THÁI
        // =====================================================================

        /// <summary>
        /// Chuyển sang trạng thái mới. Gọi Exit ở state cũ và Enter ở state mới.
        /// </summary>
        protected virtual void TransitionToState(EnemyState newState)
        {
            if (isDead && newState != EnemyState.Dead) return;

            // Thoát state cũ
            OnStateExit(CurrentState);

            EnemyState previousState = CurrentState;
            CurrentState = newState;

            // Vào state mới
            OnStateEnter(newState, previousState);
        }

        /// <summary>
        /// Được gọi khi THOÁT khỏi một trạng thái. Override để dọn dẹp logic riêng.
        /// </summary>
        protected virtual void OnStateExit(EnemyState exitingState)
        {
            // Lớp con có thể override để thêm logic exit tùy chỉnh
        }

        /// <summary>
        /// Được gọi khi BẮT ĐẦU một trạng thái mới. Override để khởi tạo logic riêng.
        /// </summary>
        protected virtual void OnStateEnter(EnemyState enteringState, EnemyState previousState)
        {
            // Lớp con có thể override để thêm animation trigger, SFX, v.v.
        }

        // =====================================================================
        //  PATROL STATE – Tuần tra qua lại, dò mép vực & tường
        // =====================================================================

        /// <summary>
        /// Logic tuần tra: Di chuyển theo facingDirection, dò mép vực và tường.
        /// Nếu phát hiện Player trong detectionRange → chuyển Chase.
        /// </summary>
        protected virtual void PatrolLogic()
        {
            // --- Dò mép vực và tường → Flip nếu cần ---
            if (IsAtEdge() || IsWallAhead())
            {
                Flip();
            }

            // --- Di chuyển tuần tra ---
            MoveHorizontal(patrolSpeed);

            // --- Kiểm tra phát hiện Player ---
            if (DetectPlayer())
            {
                TransitionToState(EnemyState.Chase);
            }
        }

        // =====================================================================
        //  CHASE STATE – Rượt đuổi Player, vẫn dò mép vực
        // =====================================================================

        /// <summary>
        /// Logic rượt đuổi: Hướng về phía Player, di chuyển nhanh hơn.
        /// Vẫn dò mép vực để không rơi xuống hố.
        /// Nếu Player ra khỏi detectionRange → Patrol.
        /// Nếu Player lọt vào attackRange → Attack.
        /// </summary>
        protected virtual void ChaseLogic()
        {
            if (playerTarget == null)
            {
                TransitionToState(EnemyState.Patrol);
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            // --- Lost Target: Player ra khỏi tầm phát hiện ---
            if (distanceToPlayer > detectionRange)
            {
                playerTarget = null;
                TransitionToState(EnemyState.Patrol);
                return;
            }

            // --- In Range: Đủ gần để tấn công ---
            if (distanceToPlayer <= attackRange)
            {
                TransitionToState(EnemyState.Attack);
                return;
            }

            // --- Hướng về phía Player ---
            FaceTarget(playerTarget.position);

            // --- Dò mép vực: nếu gặp vực thì dừng, không nhảy xuống ---
            if (IsAtEdge())
            {
                StopMovement();
                return;
            }

            // --- Di chuyển rượt đuổi ---
            MoveHorizontal(moveSpeed);
        }

        // =====================================================================
        //  ATTACK STATE – Dừng, đánh, chờ cooldown rồi quay lại Chase
        // =====================================================================

        /// <summary>
        /// Logic tấn công: Dừng di chuyển, thực hiện đòn đánh nếu hết cooldown.
        /// Sau khi đánh xong (hết attackCooldown) → chuyển Chase.
        /// </summary>
        protected virtual void AttackLogic()
        {
            // Dừng di chuyển khi đang tấn công
            StopMovement();

            // Quay mặt về phía Player nếu còn target
            if (playerTarget != null)
            {
                FaceTarget(playerTarget.position);
            }

            // Cooldown đã hết → Thực hiện tấn công
            if (attackCooldownTimer <= 0f)
            {
                PerformAttack();
                attackCooldownTimer = attackCooldown;
            }

            // Sau khi đặt cooldown, kiểm tra chuyển state
            // Attack End: hết cooldown → quay lại Chase
            if (attackCooldownTimer <= 0f)
            {
                TransitionToState(EnemyState.Chase);
                return;
            }

            // Nếu Player đã ra khỏi attackRange trong lúc chờ cooldown → Chase
            if (playerTarget != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                if (distanceToPlayer > attackRange && attackCooldownTimer <= 0f)
                {
                    TransitionToState(EnemyState.Chase);
                }
            }
            else
            {
                // Mất target hoàn toàn → Patrol
                TransitionToState(EnemyState.Patrol);
            }
        }

        /// <summary>
        /// Thực hiện đòn tấn công: Tìm Player trong attackRange và gọi TakeDamage qua IDamageable.
        /// Override ở lớp con để thêm animation, SFX, projectile, v.v.
        /// </summary>
        protected virtual void PerformAttack()
        {
            if (playerTarget == null) return;

            // Kiểm tra khoảng cách lần cuối trước khi gây sát thương
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer > attackRange) return;

            // Lấy IDamageable từ Player và gây sát thương
            IDamageable damageable = playerTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Tính hướng knockback dựa trên facingDirection
                Vector2 deliveredKnockback = new Vector2(
                    attackKnockback.x * facingDirection,
                    attackKnockback.y
                );
                damageable.TakeDamage(attackDamage, deliveredKnockback);
            }
        }

        // =====================================================================
        //  HIT STATE – Nhận sát thương (IDamageable Implementation)
        // =====================================================================

        /// <summary>
        /// Gây sát thương lên quái vật (không có knockback).
        /// Có thể bị gọi từ BẤT KỲ state nào → ngắt hành động ngay lập tức.
        /// </summary>
        public void TakeDamage(float damage)
        {
            TakeDamage(damage, Vector2.zero);
        }

        /// <summary>
        /// Gây sát thương lên quái vật kèm knockback.
        /// NGAY LẬP TỨC ngắt hành động hiện tại, trừ máu, chuyển sang Hit state.
        /// </summary>
        public void TakeDamage(float damage, Vector2 knockback)
        {
            if (isDead || damage <= 0f) return;

            // --- Trừ máu ---
            currentHP -= damage;
            currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

            Debug.Log($"[EnemyBase] {gameObject.name} nhận {damage} sát thương. HP: {currentHP}/{maxHP}");

            // Phát sự kiện damage
            OnDamageTaken?.Invoke(damage, currentHP);

            // --- Áp dụng Knockback ---
            if (knockback != Vector2.zero && rb != null)
            {
                rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
            }

            // --- Ngắt Coroutine Hit cũ nếu đang bị hit liên tiếp ---
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }

            // --- Kiểm tra chết ---
            if (currentHP <= 0f)
            {
                TransitionToState(EnemyState.Dead);
                HandleDeath();
                return;
            }

            // --- Chuyển sang Hit state và bắt đầu Coroutine stagger ---
            TransitionToState(EnemyState.Hit);
            hitCoroutine = StartCoroutine(HitStaggerCoroutine());
        }

        /// <summary>
        /// Coroutine mô phỏng thời gian khựng lại (stagger) khi bị trúng đòn.
        /// Sau khi hết stagger: nếu HP > 0 → Chase (aggro), nếu HP <= 0 → Dead.
        /// </summary>
        protected virtual IEnumerator HitStaggerCoroutine()
        {
            // Dừng di chuyển trong lúc bị hit
            StopMovement();

            // Chờ hết thời gian stagger
            yield return new WaitForSeconds(hitStaggerDuration);

            hitCoroutine = null;

            // Sau khi hết stagger, kiểm tra HP
            if (isDead) yield break;

            if (currentHP > 0f)
            {
                // Còn sống → Aggro Player, chuyển Chase
                // Cố gắng tìm lại Player nếu chưa có target
                if (playerTarget == null)
                {
                    DetectPlayer();
                }
                TransitionToState(EnemyState.Chase);
            }
            else
            {
                // Đã chết (trường hợp bị damage thêm trong lúc stagger)
                TransitionToState(EnemyState.Dead);
                HandleDeath();
            }
        }

        // =====================================================================
        //  DEAD STATE – Khóa logic, cleanup
        // =====================================================================

        /// <summary>
        /// Xử lý khi quái chết: khóa mọi logic, dừng vật lý, phát event.
        /// Override để thêm animation chết, drop loot, VFX, v.v.
        /// </summary>
        protected virtual void HandleDeath()
        {
            if (isDead) return;

            isDead = true;
            CurrentState = EnemyState.Dead;

            Debug.Log($"[EnemyBase] {gameObject.name} đã chết!");

            // Dừng mọi di chuyển
            StopMovement();

            // Dừng mô phỏng vật lý
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false;
            }

            // Dừng mọi Coroutine đang chạy
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }

            // Phát sự kiện chết
            OnDied?.Invoke();

            // Vô hiệu hóa Collider để không cản trở
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Lớp con override để thêm: animation die, drop loot, Destroy/Disable sau delay...
            OnDeathFinalize();
        }

        /// <summary>
        /// Hook cuối cùng sau khi chết. Override để Destroy, Disable, drop items, v.v.
        /// Mặc định: Disable GameObject sau 2 giây (chờ animation).
        /// </summary>
        protected virtual void OnDeathFinalize()
        {
            StartCoroutine(DisableAfterDelay(2f));
        }

        private IEnumerator DisableAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        // =====================================================================
        //  MOVEMENT HELPERS
        // =====================================================================

        /// <summary>
        /// Di chuyển ngang theo facingDirection với tốc độ chỉ định.
        /// </summary>
        protected virtual void MoveHorizontal(float speed)
        {
            if (rb == null) return;
            rb.velocity = new Vector2(facingDirection * speed, rb.velocity.y);
        }

        /// <summary>
        /// Dừng di chuyển ngang ngay lập tức (giữ nguyên vận tốc dọc cho trọng lực).
        /// </summary>
        protected virtual void StopMovement()
        {
            if (rb == null) return;
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        // =====================================================================
        //  DETECTION & SENSING
        // =====================================================================

        /// <summary>
        /// Phát hiện Player trong phạm vi detectionRange bằng Physics2D.OverlapCircle.
        /// Nếu tìm thấy → cache playerTarget và trả về true.
        /// </summary>
        protected virtual bool DetectPlayer()
        {
            Collider2D playerCollider = Physics2D.OverlapCircle(
                transform.position,
                detectionRange,
                playerLayer
            );

            if (playerCollider != null)
            {
                playerTarget = playerCollider.transform;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dò mép vực phía trước: bắn Raycast xuống dưới từ edgeCheckPoint.
        /// Trả về true nếu KHÔNG có mặt đất phía trước (= sắp rơi xuống vực).
        /// </summary>
        protected virtual bool IsAtEdge()
        {
            Vector2 checkPosition;

            if (edgeCheckPoint != null)
            {
                checkPosition = edgeCheckPoint.position;
            }
            else
            {
                // Fallback: tính vị trí kiểm tra tự động dựa trên Collider
                Collider2D col = GetComponent<Collider2D>();
                float offsetX = col != null ? col.bounds.extents.x : 0.5f;
                float bottomY = col != null ? col.bounds.min.y : transform.position.y - 0.5f;
                checkPosition = new Vector2(
                    transform.position.x + (facingDirection * offsetX),
                    bottomY
                );
            }

            // Bắn Raycast thẳng xuống, nếu không chạm Ground → đang ở mép vực
            RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, edgeCheckDistance, groundLayer);
            return hit.collider == null;
        }

        /// <summary>
        /// Dò tường phía trước: bắn Raycast ngang theo facingDirection.
        /// Trả về true nếu có tường chắn phía trước.
        /// </summary>
        protected virtual bool IsWallAhead()
        {
            Vector2 origin = transform.position;
            Vector2 direction = Vector2.right * facingDirection;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, groundLayer);
            return hit.collider != null;
        }

        // =====================================================================
        //  FLIP & FACE TARGET
        // =====================================================================

        /// <summary>
        /// Lật hướng quay mặt của quái vật (đổi localScale.x – đồng nhất với PlayerController).
        /// </summary>
        protected virtual void Flip()
        {
            facingDirection *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        /// <summary>
        /// Quay mặt về phía một vị trí mục tiêu. Flip nếu cần.
        /// </summary>
        protected virtual void FaceTarget(Vector3 targetPosition)
        {
            float directionToTarget = targetPosition.x - transform.position.x;

            if (directionToTarget > 0f && facingDirection < 0)
            {
                Flip();
            }
            else if (directionToTarget < 0f && facingDirection > 0)
            {
                Flip();
            }
        }

        // =====================================================================
        //  GIZMOS – Trực quan hóa phạm vi trong Unity Editor
        // =====================================================================

        protected virtual void OnDrawGizmosSelected()
        {
            // Vòng tròn Detection Range (xanh lá)
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Vòng tròn Attack Range (đỏ)
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Raycast dò mép vực (vàng)
            Gizmos.color = Color.yellow;
            Vector2 edgePos;
            if (edgeCheckPoint != null)
            {
                edgePos = edgeCheckPoint.position;
            }
            else
            {
                Collider2D col = GetComponent<Collider2D>();
                float offsetX = col != null ? col.bounds.extents.x : 0.5f;
                float bottomY = col != null ? col.bounds.min.y : transform.position.y - 0.5f;
                edgePos = new Vector2(
                    transform.position.x + (facingDirection * offsetX),
                    bottomY
                );
            }
            Gizmos.DrawLine(edgePos, edgePos + Vector2.down * edgeCheckDistance);

            // Raycast dò tường (cyan)
            Gizmos.color = Color.cyan;
            Vector2 wallOrigin = transform.position;
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * facingDirection * wallCheckDistance);
        }
    }
}
