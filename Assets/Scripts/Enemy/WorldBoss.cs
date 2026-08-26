using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Roguelite.Combat;
using Roguelite.Core;
using Roguelite.RoomSystem;
using Roguelite.SaveSystem;

namespace Roguelite.Enemy
{
    /// <summary>
    /// WorldBoss - Boss nâng cao phát triển từ BossBase.
    /// Tính năng đặc biệt:
    /// 1. Nhảy vượt chướng ngại vật và leo địa hình cao để truy đuổi Player.
    /// 2. Cơ chế tích tụ đòn đánh: Sau 4 đòn tấn công thông thường sẽ giải phóng chiêu Mưa Thiên Thạch (Meteor Rain) hủy diệt.
    /// 3. Tự động tương thích với hệ thống HealthBar, Phase Transition (Enrage), Room Clear khi bị hạ gục.
    /// 4. Hỗ trợ Procedural Fallback Visuals (tự tạo hiệu ứng cảnh báo & thiên thạch nếu chưa gắn Prefab trong Inspector).
    /// </summary>
    public class WorldBoss : BossBase
    {
        public const string VERSION = "1.8.1";

        #region ====== SERIALIZE FIELDS - HEALTHBAR & DISPLAY ======

        [Header("===== WorldBoss UI & Visuals =====")]
        [Tooltip("Prefab HealthBar hiển thị trên đầu Boss (tự động gán nếu để trống).")]
        [SerializeField] private HealthBar healthBar;

        [Tooltip("Khoảng cách bổ sung phía trên sprite Boss cho HealthBar.")]
        [SerializeField] private float healthBarPadding = 0.8f;

        [Tooltip("Kích thước thanh máu trong world units (rộng x cao).")]
        [SerializeField] private Vector2 healthBarWorldSize = new Vector2(3.5f, 0.5f);

        #endregion

        #region ====== SERIALIZE FIELDS - JUMP & TERRAIN NAVIGATION ======

        [Header("===== Jump & Multi-Terrain Navigation =====")]
        [Tooltip("Lực nhảy theo phương thẳng đứng (Y velocity).")]
        [SerializeField] private float jumpForce = 14f;

        [Tooltip("Tốc độ đẩy ngang khi thực hiện cú nhảy vượt chướng ngại (X velocity).")]
        [SerializeField] private float jumpForwardSpeed = 5.5f;

        [Tooltip("Thời gian hồi giữa các lần nhảy (giây).")]
        [SerializeField] private float jumpCooldown = 2.0f;

        [Tooltip("Chênh lệch độ cao Y tối thiểu giữa Player và Boss để kích hoạt nhảy lên địa hình cao.")]
        [SerializeField] private float jumpHeightThreshold = 1.2f;

        [Tooltip("Bán kính vòng tròn kiểm tra tiếp đất ở chân Boss.")]
        [SerializeField] private float groundCheckRadius = 0.35f;

        [Tooltip("Vị trí offset kiểm tra tiếp đất so với tâm Boss.")]
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.8f);

        #endregion

        #region ====== SERIALIZE FIELDS - METEOR RAIN ULTIMATE ======

        [Header("===== Meteor Rain Ultimate Settings =====")]
        [Tooltip("Số lần tấn công thông thường cần tích lũy trước khi thi triển Mưa Thiên Thạch.")]
        [SerializeField] private int attacksBeforeMeteor = 4;

        [Tooltip("Sát thương mỗi quả thiên thạch khi va chạm mặt đất.")]
        [SerializeField] private float meteorDamage = 35f;

        [Tooltip("Số lượng thiên thạch cơ bản trong 1 đợt mưa (Phase cao sẽ tăng thêm).")]
        [SerializeField] private int baseMeteorCount = 5;

        [Tooltip("Bán kính phân tán thiên thạch xung quanh mục tiêu.")]
        [SerializeField] private float meteorSpawnRadius = 7f;

        [Tooltip("Thời gian xuất hiện vòng tròn cảnh báo nguy hiểm (Telegraph) trước khi thiên thạch rơi trúng.")]
        [SerializeField] private float meteorTelegraphDuration = 1.0f;

        [Tooltip("Tốc độ rơi của thiên thạch (đơn vị/giây).")]
        [SerializeField] private float meteorFallSpeed = 18f;

        [Tooltip("Bán kính nổ gây sát thương diện rộng (AoE) của mỗi quả thiên thạch.")]
        [SerializeField] private float meteorImpactRadius = 2.2f;

        [Tooltip("Lực đẩy lùi (Knockback) khi trúng thiên thạch.")]
        [SerializeField] private Vector2 meteorKnockback = new Vector2(5f, 7f);

        [Tooltip("Thời gian khóa Boss đứng tụ chiêu khi gọi mưa thiên thạch (giây).")]
        [SerializeField] private float meteorCastDuration = 1.5f;

        [Header("===== Prefabs & Audio (Optional - Có Fallback) =====")]
        [Tooltip("Prefab của quả thiên thạch rơi (Nếu để trống, sẽ tự tạo quả cầu lửa Procedural).")]
        [SerializeField] private GameObject meteorPrefab;

        [Tooltip("Prefab vòng tròn cảnh báo màu đỏ (Nếu để trống, sẽ tự tạo vòng cảnh báo Procedural).")]
        [SerializeField] private GameObject telegraphMarkerPrefab;

        [Tooltip("Prefab hiệu ứng vụ nổ khi thiên thạch chạm đất (Tùy chọn).")]
        [SerializeField] private GameObject explosionVfxPrefab;

        [Tooltip("Âm thanh khi Boss bắt đầu gầm thét gọi mưa thiên thạch (Tùy chọn).")]
        [SerializeField] private AudioClip meteorCastSound;

        [Tooltip("Âm thanh khi quả thiên thạch phát nổ chạm đất (Tùy chọn).")]
        [SerializeField] private AudioClip meteorImpactSound;

        #endregion

        #region ====== RUNTIME VARIABLES ======

        private int currentAttackCount = 0;
        private bool isCastingMeteor = false;
        private bool isGrounded = true;
        private bool isJumping = false;
        private float jumpCooldownTimer = 0f;

        // Biến theo dõi và giải cứu chống kẹt vào vật thể
        private float stuckTimer = 0f;
        private float lastPositionX = 0f;

        private bool ownsHealthBarInstance = false;
        private Material originalMaterial;

        // Shared static assets tối ưu hiệu năng cho Mưa Thiên Thạch (tránh GC Alloc & lag FPS)
        private static Sprite s_SharedMeteorSprite;
        private static Sprite s_SharedExplosionSprite;
        private static Material s_SharedSpriteMaterial;
        private static float s_LastHitStopTime = 0f;

        // Biến Dummy Mode cho Sandbox Tool
        private bool isDummyMode = false;
        public bool IsDummyMode { get => isDummyMode; set => isDummyMode = value; }

        /// <summary>Số đòn đánh đã tích lũy hiện tại.</summary>
        public int CurrentAttackCount => currentAttackCount;

        /// <summary>Ngưỡng đòn đánh để kích hoạt Mưa Thiên Thạch.</summary>
        public int AttacksBeforeMeteor => attacksBeforeMeteor;

        /// <summary>Trạng thái Boss đang thi triển tuyệt chiêu Mưa Thiên Thạch.</summary>
        public bool IsCastingMeteor => isCastingMeteor;

        /// <summary>Trạng thái Boss có đang đứng trên mặt đất hay không.</summary>
        public bool IsGrounded => isGrounded;

        #endregion

        // =====================================================================
        //  UNITY LIFECYCLE
        // =====================================================================

        protected override void Start()
        {
            base.Start();

            if (spriteRenderer != null)
            {
                originalMaterial = spriteRenderer.material;
            }

            OnPhaseChanged += HandlePhaseChanged;
            SetupHealthBar();
            OnDamageTaken += UpdateHealthBar;

            // Thiết lập vật lý không ma sát để tránh Boss bị dính/kẹt vào bề mặt tường
            SetupFrictionlessPhysics();

            // Khởi tạo trước shared resources cho VFX để tránh lag khi Boss ra chiêu
            EnsureSharedResources();

            // Tự động tìm kiếm Player trên toàn bộ phòng Boss và vào trạng thái Chase ngay lập tức
            AcquirePlayerTarget();
            if (playerTarget != null && IsTargetAlive())
            {
                FaceTarget(playerTarget.position);
                TransitionToState(EnemyState.Chase);
            }
        }

        protected override void Update()
        {
            if (isDead) return;

            // Cập nhật kiểm tra tiếp đất
            CheckGroundedStatus();

            // Giảm hồi chiêu nhảy
            if (jumpCooldownTimer > 0f)
            {
                jumpCooldownTimer -= Time.deltaTime;
            }

            // Nếu đang tụ chiêu Mưa Thiên Thạch, khóa di chuyển và AI thông thường
            if (isCastingMeteor)
            {
                StopMovement();
                return;
            }

            base.Update();

            // Cập nhật Animator cho trạng thái nhảy nếu có
            if (anim != null)
            {
                anim.SetBool("isGrounded", isGrounded);
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

            if (spriteRenderer != null && originalMaterial != null)
            {
                spriteRenderer.material = originalMaterial;
            }
        }

        // =====================================================================
        //  GROUND & JUMP SYSTEM (DI CHUYỂN ĐA ĐỊA HÌNH & THOÁT STATE KHI TIẾP ĐẤT)
        // =====================================================================

        private void CheckGroundedStatus()
        {
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            Collider2D groundCol = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);

            bool wasGrounded = isGrounded;
            isGrounded = (groundCol != null);

            // Khi vừa tiếp đất trở lại sau khi nhảy hoặc bị hất trên không
            if (!wasGrounded && isGrounded)
            {
                OnLanded();
            }
        }

        /// <summary>
        /// Xử lý thoát và chuyển trạng thái ngay khi Boss tiếp đất.
        /// </summary>
        private void OnLanded()
        {
            isJumping = false;

            if (anim != null)
            {
                anim.SetTrigger("AI_land");
            }

            Debug.Log($"[WorldBoss] {gameObject.name} đã tiếp đất (isGrounded = true). Kiểm tra thoát/chuyển state.");

            // 1. Nếu đang ở trạng thái Hit (bị khựng/đẩy lùi trên không), thoát Hit và chuyển Chase/Idle ngay
            if (CurrentState == EnemyState.Hit)
            {
                if (hitCoroutine != null)
                {
                    StopCoroutine(hitCoroutine);
                    hitCoroutine = null;
                }

                if (playerTarget != null && IsTargetAlive())
                {
                    TransitionToState(EnemyState.Chase);
                }
                else
                {
                    TransitionToState(EnemyState.Idle);
                }
                return;
            }

            // 2. Nếu đang rượt đuổi (Chase) hoặc phát hiện mục tiêu khi tiếp đất:
            if (playerTarget != null && IsTargetAlive())
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                FaceTarget(playerTarget.position);

                if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
                {
                    TransitionToState(EnemyState.Attack);
                    return;
                }

                if (CurrentState != EnemyState.Chase && CurrentState != EnemyState.Attack)
                {
                    TransitionToState(EnemyState.Chase);
                }
            }
        }

        /// <summary>
        /// Ghi đè TransitionToState để chặn việc chuyển sang các state đòi hỏi mặt đất (như Idle đứng yên, Attack cận chiến) khi đang ở trên không.
        /// </summary>
        protected override void TransitionToState(EnemyState newState)
        {
            if (isDead && newState != EnemyState.Dead) return;

            // Nếu đang ở trên không và chưa tiếp đất:
            if (!isGrounded && newState != EnemyState.Dead && newState != EnemyState.Hit)
            {
                // Tránh đứng khựng (Idle) giữa không trung; chờ tiếp đất trong OnLanded()
                if (newState == EnemyState.Idle)
                {
                    return;
                }

                // Không vào trạng thái Attack cận chiến khi đang bay lơ lửng
                if (newState == EnemyState.Attack)
                {
                    return;
                }
            }

            base.TransitionToState(newState);
        }

        protected override void StopMovement()
        {
            if (rb == null) return;

            // Nếu đang trên không và đang trong quỹ đạo nhảy, không khựng đứng lại đột ngột
            if (!isGrounded && isJumping) return;

            base.StopMovement();
        }

        /// <summary>
        /// Tự động gán PhysicsMaterial2D không ma sát để Boss trượt mượt mà dọc theo tường và vật cản.
        /// </summary>
        private void SetupFrictionlessPhysics()
        {
            PhysicsMaterial2D frictionlessMat = new PhysicsMaterial2D("Frictionless_Boss")
            {
                friction = 0f,
                bounciness = 0f
            };

            if (rb != null)
            {
                rb.sharedMaterial = frictionlessMat;
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                if (col != null && !col.isTrigger)
                {
                    col.sharedMaterial = frictionlessMat;
                }
            }
        }

        /// <summary>
        /// Dò tìm chướng ngại vật & tường đa điểm (Chân, Bụng, Đầu) với khoảng cách thích ứng theo kích thước Boss.
        /// </summary>
        protected override bool IsWallAhead()
        {
            Collider2D col = GetComponent<Collider2D>();
            float extentsX = col != null ? col.bounds.extents.x : 0.6f;
            float extentsY = col != null ? col.bounds.extents.y : 1.0f;

            // Điểm bắt đầu bắn tia: mép ngoài cùng của Collider theo hướng nhìn
            float originX = transform.position.x + (facingDirection * (extentsX + 0.05f));
            float checkDist = Mathf.Max(wallCheckDistance, 1.2f);
            Vector2 direction = Vector2.right * facingDirection;

            // Dò 3 độ cao: Chân (bậc thấp), Bụng (vật cản vừa), Đầu (tường cao)
            Vector2 footPos = new Vector2(originX, transform.position.y - (extentsY * 0.6f));
            Vector2 waistPos = new Vector2(originX, transform.position.y);
            Vector2 headPos = new Vector2(originX, transform.position.y + (extentsY * 0.6f));

            RaycastHit2D hitFoot = Physics2D.Raycast(footPos, direction, checkDist, groundLayer);
            RaycastHit2D hitWaist = Physics2D.Raycast(waistPos, direction, checkDist, groundLayer);
            RaycastHit2D hitHead = Physics2D.Raycast(headPos, direction, checkDist, groundLayer);

            return (hitFoot.collider != null || hitWaist.collider != null || hitHead.collider != null);
        }

        /// <summary>
        /// Thực hiện cú nhảy để vượt tường, vượt vực hoặc nhảy lên bục cao.
        /// </summary>
        public void PerformJump(float forwardSpeed, float verticalForce, bool forceJump = false)
        {
            if (rb == null || (!isGrounded && !forceJump)) return;
            if (!forceJump && jumpCooldownTimer > 0f) return;

            isJumping = true;
            jumpCooldownTimer = jumpCooldown;

            // Áp dụng vận tốc nhảy thẳng đứng + quán tính lao về phía trước
            rb.velocity = new Vector2(facingDirection * forwardSpeed, verticalForce);

            if (anim != null)
            {
                anim.SetTrigger("AI_jump");
            }

            Debug.Log($"[WorldBoss] {gameObject.name} thực hiện cú nhảy di chuyển đa địa hình! Force: (X: {facingDirection * forwardSpeed}, Y: {verticalForce})");
        }

        // =====================================================================
        //  TARGET ACQUISITION & CHASE LOGIC OVERRIDE (ARENA AGGRO & NO DE-AGGRO)
        // =====================================================================

        /// <summary>
        /// Tự động tìm kiếm mục tiêu Player trên toàn bộ đấu trường Boss (không giới hạn bởi detectionRange nhỏ).
        /// </summary>
        public void AcquirePlayerTarget()
        {
            if (playerTarget != null && IsTargetAlive()) return;

            // 1. Dò tìm diện rộng quanh Boss (bán kính 35m bao phủ cả phòng)
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, Mathf.Max(detectionRange, 35f), playerLayer);
            if (playerCollider != null)
            {
                playerTarget = playerCollider.transform;
                return;
            }

            // 2. Tìm qua Tag "Player"
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                return;
            }

            // 3. Tìm qua PlayerController
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                playerTarget = pc.transform;
            }
        }

        protected override void ChaseLogic()
        {
            if (playerTarget == null || !IsTargetAlive())
            {
                AcquirePlayerTarget();

                if (playerTarget == null || !IsTargetAlive())
                {
                    playerTarget = null;
                    if (isGrounded)
                    {
                        TransitionToState(EnemyState.Idle);
                    }
                    return;
                }
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            // Quay mặt về phía Player liên tục
            FaceTarget(playerTarget.position);

            // Trong tầm đánh cận chiến (chỉ tấn công khi đã tiếp đất)
            if (distanceToPlayer <= attackRange)
            {
                if (isGrounded)
                {
                    if (attackCooldownTimer <= 0f)
                    {
                        FaceTarget(playerTarget.position);
                        TransitionToState(EnemyState.Attack);
                        return;
                    }
                    else
                    {
                        // Giữ mặt luôn hướng theo Player và duy trì áp lực, tránh đứng đơ nhìn nhau
                        FaceTarget(playerTarget.position);
                        if (distanceToPlayer > attackRange * 0.6f)
                        {
                            MoveHorizontal(moveSpeed * 0.55f);
                        }
                        else
                        {
                            StopMovement();
                        }
                        return;
                    }
                }
            }

            // --- KIỂM TRA CHỐNG KẸT VẬT THỂ (UNSTUCK AUTO JUMP RECOVERY) ---
            if (isGrounded && !isJumping)
            {
                float currentX = transform.position.x;
                bool isActuallyStationary = Mathf.Abs(currentX - lastPositionX) < 0.02f && Mathf.Abs(rb.velocity.x) < 0.15f;

                if (isActuallyStationary && distanceToPlayer > attackRange)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer >= 0.25f)
                    {
                        stuckTimer = 0f;
                        Debug.Log($"[WorldBoss] {gameObject.name} phát hiện bị kẹt vào vật thể! Tự động kích hoạt cú nhảy giải cứu.");
                        PerformJump(jumpForwardSpeed * 1.3f, jumpForce * 1.1f, true);
                        return;
                    }
                }
                else
                {
                    stuckTimer = Mathf.Max(0f, stuckTimer - Time.deltaTime * 2f);
                }
                lastPositionX = currentX;
            }

            // --- XỬ LÝ NHẢY VƯỢT ĐỊA HÌNH ---
            bool playerIsHigher = (playerTarget.position.y - transform.position.y) > jumpHeightThreshold;
            bool wallAhead = IsWallAhead();
            bool edgeAhead = IsAtEdge();

            // 1. Nếu Player ở bục cao hơn và có thể nhảy -> Nhảy lên
            if (playerIsHigher && isGrounded && jumpCooldownTimer <= 0f)
            {
                PerformJump(jumpForwardSpeed, jumpForce * 1.15f);
                return;
            }

            // 2. Nếu có tường chắn phía trước và đang tiếp đất -> Nhảy qua tường
            if (wallAhead && isGrounded && jumpCooldownTimer <= 0f)
            {
                PerformJump(jumpForwardSpeed * 1.15f, jumpForce);
                return;
            }

            // 3. Nếu gặp mép vực:
            if (edgeAhead)
            {
                // Nếu Player đang ở phía trước qua bên kia vực -> Nhảy qua vực
                float dirToPlayer = playerTarget.position.x - transform.position.x;
                bool playerInFront = (dirToPlayer * facingDirection) > 0f;

                if (playerInFront && isGrounded && jumpCooldownTimer <= 0f)
                {
                    PerformJump(jumpForwardSpeed * 1.25f, jumpForce * 0.9f);
                    return;
                }

                // Nếu không nhảy được, tạm dừng lại mép vực tránh rơi
                if (!isJumping)
                {
                    StopMovement();
                    return;
                }
            }

            // Nếu đang trong trạng thái nhảy trên không, duy trì vận tốc ngang
            if (isJumping)
            {
                MoveHorizontal(jumpForwardSpeed);
                return;
            }

            // Di chuyển đuổi theo mục tiêu bình thường
            MoveHorizontal(moveSpeed);
        }

        protected override void IdleLogic()
        {
            StopMovement();

            // Tự động tìm Player nếu chưa có
            AcquirePlayerTarget();

            // Nếu đã có mục tiêu còn sống -> chuyển Chase ngay lập tức, không chờ hết idleTimer
            if (playerTarget != null && IsTargetAlive())
            {
                FaceTarget(playerTarget.position);
                TransitionToState(EnemyState.Chase);
                return;
            }

            if (idleTimer > 0f)
            {
                idleTimer -= Time.deltaTime;
            }
            else
            {
                Flip();
                TransitionToState(EnemyState.Patrol);
                return;
            }
        }

        protected override void PatrolLogic()
        {
            // Tự động tìm Player khi tuần tra
            AcquirePlayerTarget();
            if (playerTarget != null && IsTargetAlive())
            {
                FaceTarget(playerTarget.position);
                TransitionToState(EnemyState.Chase);
                return;
            }

            // Kiểm tra mép vực hoặc tường khi tuần tra
            if (IsAtEdge() || IsWallAhead())
            {
                // Khi tuần tra, gặp vật cản có tỉ lệ nhảy hoặc quay đầu
                if (IsWallAhead() && isGrounded && jumpCooldownTimer <= 0f && UnityEngine.Random.value > 0.5f)
                {
                    PerformJump(jumpForwardSpeed * 0.7f, jumpForce);
                    return;
                }

                TransitionToState(EnemyState.Idle);
                return;
            }

            // Giới hạn phạm vi tuần tra
            float currentX = transform.position.x;
            float targetCenterX = patrolCenter.x;
            float distanceTravelled = Mathf.Abs(currentX - targetCenterX);

            if (distanceTravelled >= patrolRange)
            {
                bool isMovingAway = (currentX - targetCenterX) * facingDirection > 0;
                if (isMovingAway)
                {
                    TransitionToState(EnemyState.Idle);
                    return;
                }
            }

            if (patrolTimer > 0f)
            {
                patrolTimer -= Time.deltaTime;
            }
            else
            {
                TransitionToState(EnemyState.Idle);
                return;
            }

            if (isJumping)
            {
                MoveHorizontal(jumpForwardSpeed * 0.7f);
            }
            else
            {
                MoveHorizontal(patrolSpeed);
            }

            if (DetectPlayer())
            {
                if (playerTarget != null) FaceTarget(playerTarget.position);
                TransitionToState(EnemyState.Chase);
            }
        }

        // =====================================================================
        //  ATTACK SYSTEM & METEOR RAIN ULTIMATE (ĐẾM 4 ĐÒN ĐÁNH)
        // =====================================================================

        /// <summary>
        /// Override AttackLogic để KHÔNG chuyển Chase ngay lập tức.
        /// EnemyBase.AttackLogic() gọi PerformAttack() rồi TransitionToState(Chase) cùng frame,
        /// khiến animation + hitbox bị cancel trước khi kịp chạy.
        /// Fix: dùng Coroutine đợi attackLockDuration xong mới chuyển state.
        /// </summary>
        private Coroutine attackLogicCoroutine;

        protected override void AttackLogic()
        {
            // Chỉ bắt đầu đòn đánh MỘT LẦN, không gọi lại mỗi frame
            if (attackLogicCoroutine != null) return;

            if (playerTarget != null)
            {
                FaceTarget(playerTarget.position);
            }
            StopMovement();
            attackLogicCoroutine = StartCoroutine(AttackLogicCoroutine());
        }

        private IEnumerator AttackLogicCoroutine()
        {
            // Đảm bảo xoay đúng hướng mục tiêu trước khi ra đòn
            if (playerTarget != null)
            {
                FaceTarget(playerTarget.position);
            }

            // Thực hiện đòn đánh
            PerformAttack();

            // Xác định thời gian khóa đòn đánh (đồng bộ với thời lượng animation vung đòn)
            float lockDuration = 0.6f;
            if (IsAttackingPattern && ActivePattern != null)
            {
                lockDuration = ActivePattern.AttackLockDuration;
            }
            else
            {
                // Độ dài của clip animation attack là 0.6s
                lockDuration = 0.6f;
            }

            yield return new WaitForSeconds(lockDuration);

            // Sau khi animation đòn đánh hoàn tất, đặt cooldown ngắn để Boss tiếp tục truy đuổi linh hoạt
            attackCooldownTimer = attackCooldown;

            // Đảm bảo tắt hitbox an toàn khi kết thúc thời gian đòn đánh
            CleanupAttackHitboxes();

            attackLogicCoroutine = null;

            // Sau khi đòn đánh hoàn tất, chuyển về Chase hoặc Idle
            if (!isDead)
            {
                if (playerTarget != null && IsTargetAlive())
                {
                    TransitionToState(EnemyState.Chase);
                }
                else
                {
                    TransitionToState(EnemyState.Idle);
                }
            }
        }

        protected override void PerformAttack()
        {
            if (isCastingMeteor) return;

            currentAttackCount++;
            Debug.Log($"[WorldBoss] {gameObject.name} thực hiện đòn tấn công số [{currentAttackCount}/{attacksBeforeMeteor}]");

            // Kiểm tra kích hoạt Mưa Thiên Thạch sau 4 đòn đánh
            if (currentAttackCount >= attacksBeforeMeteor)
            {
                currentAttackCount = 0;
                StartCoroutine(MeteorRainUltimateCoroutine());
            }
            else
            {
                // Thực hiện đòn đánh thường / Pattern theo BossBase
                base.PerformAttack();
            }
        }

        private IEnumerator MeteorRainUltimateCoroutine()
        {
            // Đảm bảo Boss đã tiếp đất trước khi bắt đầu tụ chiêu
            while (!isGrounded)
            {
                yield return null;
            }

            isCastingMeteor = true;
            StopMovement();

            Debug.Log($"[WorldBoss] 🔥 {gameObject.name} BẮT ĐẦU THI TRIỂN MƯA THIÊN THẠCH (METEOR RAIN)! 🔥");

            // Kích hoạt animation gồng chiêu / tụ lực
            if (anim != null)
            {
                anim.SetTrigger("AI_cast");
            }

            // Phát âm thanh gầm/tụ chiêu nếu có
            if (meteorCastSound != null)
            {
                AudioSource.PlayClipAtPoint(meteorCastSound, transform.position);
            }

            // Tính toán số lượng thiên thạch tối ưu theo Phase (6 đến 10 quả, uy lực và không gây lag)
            int totalMeteors = Mathf.Clamp(baseMeteorCount + (CurrentPhase * 2), 6, 10);
            float effectiveRadius = Mathf.Min(meteorSpawnRadius, 12f);

            // Xác định tâm dội thiên thạch (ưu tiên vị trí Player)
            Vector3 centerTargetPos = playerTarget != null ? playerTarget.position : (transform.position + Vector3.right * facingDirection * 3f);

            // 1. Tạo danh sách các điểm rơi thiên thạch
            List<Vector3> impactPoints = new List<Vector3>();
            impactPoints.Add(centerTargetPos); // Quả đầu tiên luôn nhắm thẳng vào Player

            for (int i = 1; i < totalMeteors; i++)
            {
                float randomOffsetX = UnityEngine.Random.Range(-effectiveRadius, effectiveRadius);
                float randomOffsetY = UnityEngine.Random.Range(-1.5f, 2.5f);
                Vector3 targetPoint = centerTargetPos + new Vector3(randomOffsetX, randomOffsetY, 0f);

                // Dò tìm mặt đất bên dưới điểm rơi
                RaycastHit2D hit = Physics2D.Raycast(targetPoint + Vector3.up * 2f, Vector2.down, 10f, groundLayer);
                if (hit.collider != null)
                {
                    targetPoint = hit.point;
                }

                impactPoints.Add(targetPoint);
            }

            // 2. Tạo các vòng tròn cảnh báo nguy hiểm (Telegraph)
            List<GameObject> telegraphObjects = new List<GameObject>();
            foreach (Vector3 point in impactPoints)
            {
                GameObject marker = SpawnTelegraphMarker(point);
                if (marker != null)
                {
                    telegraphObjects.Add(marker);
                }
            }

            // Chờ thời gian hiển thị vòng cảnh báo
            yield return new WaitForSeconds(meteorTelegraphDuration);

            // 3. Cho các quả thiên thạch dội xuống (giãn cách nhẹ 0.15s)
            for (int i = 0; i < impactPoints.Count; i++)
            {
                Vector3 impactPos = impactPoints[i];
                GameObject marker = i < telegraphObjects.Count ? telegraphObjects[i] : null;

                StartCoroutine(DropSingleMeteor(impactPos, marker));

                yield return new WaitForSeconds(0.15f);
            }

            // Chờ hết thời gian tụ chiêu
            yield return new WaitForSeconds(meteorCastDuration);

            isCastingMeteor = false;
            Debug.Log($"[WorldBoss] Hoàn tất đợt Mưa Thiên Thạch. Trở lại trạng thái bình thường.");
        }

        private IEnumerator DropSingleMeteor(Vector3 targetImpactPos, GameObject associatedMarker)
        {
            // Điểm xuất phát của thiên thạch từ trên trời
            float spawnHeight = 12f;
            float spawnOffsetX = UnityEngine.Random.Range(-2.5f, 2.5f);
            Vector3 startPos = targetImpactPos + new Vector3(spawnOffsetX, spawnHeight, 0f);

            GameObject meteorInstance = null;

            if (meteorPrefab != null)
            {
                meteorInstance = Instantiate(meteorPrefab, startPos, Quaternion.identity);
            }
            else
            {
                // Fallback Procedural Meteor (Shared Resource - Zero GC Alloc)
                meteorInstance = CreateProceduralMeteorObject(startPos);
            }

            // Tính hướng rơi
            float distance = Vector3.Distance(startPos, targetImpactPos);
            float duration = distance / Mathf.Max(meteorFallSpeed, 10f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (meteorInstance != null)
                {
                    meteorInstance.transform.position = Vector3.Lerp(startPos, targetImpactPos, t);
                }
                yield return null;
            }

            // Xóa thiên thạch và vòng cảnh báo khi chạm đất
            if (meteorInstance != null)
            {
                Destroy(meteorInstance);
            }
            if (associatedMarker != null)
            {
                Destroy(associatedMarker);
            }

            // Xử lý nổ & gây sát thương diện rộng
            HandleMeteorExplosion(targetImpactPos);
        }

        private void HandleMeteorExplosion(Vector3 impactPos)
        {
            // Hiệu ứng nổ VFX
            if (explosionVfxPrefab != null)
            {
                Instantiate(explosionVfxPrefab, impactPos, Quaternion.identity);
            }
            else
            {
                CreateProceduralExplosionEffect(impactPos);
            }

            // Âm thanh nổ
            if (meteorImpactSound != null)
            {
                AudioSource.PlayClipAtPoint(meteorImpactSound, impactPos);
            }

            // Tìm và gây sát thương cho các đối tượng trong vùng nổ
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(impactPos, meteorImpactRadius, playerLayer);
            bool hitPlayer = false;

            foreach (Collider2D hit in hitColliders)
            {
                if (hit.gameObject == gameObject) continue; // Không tự gây sát thương cho Boss

                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = hit.GetComponentInParent<IDamageable>();
                }

                if (damageable != null)
                {
                    hitPlayer = true;

                    // Tính hướng đẩy văng ra ngoài tâm vụ nổ
                    Vector2 knockbackDir = (hit.transform.position - impactPos).normalized;
                    if (knockbackDir == Vector2.zero) knockbackDir = Vector2.up;

                    Vector2 appliedKnockback = new Vector2(
                        knockbackDir.x * meteorKnockback.x,
                        meteorKnockback.y
                    );

                    damageable.TakeDamage(meteorDamage, appliedKnockback);
                    Debug.Log($"[WorldBoss] 💥 Thiên thạch phát nổ trúng {hit.name}! Gây {meteorDamage} sát thương.");
                }
            }

            // Chỉ kích hoạt HitStop khi thực sự TRÚNG Player và có giới hạn tần suất (tránh lag giật FPS)
            if (hitPlayer && HitStopManager.Instance != null && (Time.time - s_LastHitStopTime > 0.4f))
            {
                s_LastHitStopTime = Time.time;
                HitStopManager.Instance.LightHitStop();
            }
        }

        // =====================================================================
        //  PROCEDURAL FALLBACK VISUALS (SHARED RESOURCES - SIÊU TỐI ƯU FPS)
        // =====================================================================

        private static void EnsureSharedResources()
        {
            if (s_SharedSpriteMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader != null) s_SharedSpriteMaterial = new Material(shader);
            }

            if (s_SharedMeteorSprite == null)
            {
                Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[32 * 32];
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                        pixels[y * 32 + x] = dist <= 14f ? Color.white : Color.clear;
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply(false, true);
                s_SharedMeteorSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            }

            if (s_SharedExplosionSprite == null)
            {
                s_SharedExplosionSprite = s_SharedMeteorSprite;
            }
        }

        private GameObject SpawnTelegraphMarker(Vector3 position)
        {
            if (telegraphMarkerPrefab != null)
            {
                return Instantiate(telegraphMarkerPrefab, position, Quaternion.identity);
            }

            EnsureSharedResources();

            GameObject marker = new GameObject("TelegraphMarker_Procedural");
            marker.transform.position = position;

            LineRenderer line = marker.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            if (s_SharedSpriteMaterial != null) line.sharedMaterial = s_SharedSpriteMaterial;
            line.startColor = new Color(1f, 0.2f, 0.2f, 0.8f);
            line.endColor = new Color(1f, 0.4f, 0.1f, 0.8f);

            int segments = 16;
            line.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * (Mathf.PI * 2f / segments);
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * meteorImpactRadius, Mathf.Sin(angle) * meteorImpactRadius, 0f));
            }

            return marker;
        }

        private GameObject CreateProceduralMeteorObject(Vector3 position)
        {
            EnsureSharedResources();

            GameObject meteor = new GameObject("Meteor_Procedural");
            meteor.transform.position = position;

            SpriteRenderer sr = meteor.AddComponent<SpriteRenderer>();
            if (s_SharedSpriteMaterial != null) sr.sharedMaterial = s_SharedSpriteMaterial;
            sr.sprite = s_SharedMeteorSprite;
            sr.color = new Color(1f, 0.45f, 0.1f, 1f);
            meteor.transform.localScale = Vector3.one * 1.2f;

            // Trail Renderer tạo đuôi lửa
            TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0.05f;
            if (s_SharedSpriteMaterial != null) trail.sharedMaterial = s_SharedSpriteMaterial;
            trail.startColor = new Color(1f, 0.6f, 0.1f, 0.9f);
            trail.endColor = new Color(1f, 0.1f, 0f, 0f);

            return meteor;
        }

        private void CreateProceduralExplosionEffect(Vector3 position)
        {
            EnsureSharedResources();

            GameObject explosion = new GameObject("Explosion_Procedural");
            explosion.transform.position = position;

            SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
            if (s_SharedSpriteMaterial != null) sr.sharedMaterial = s_SharedSpriteMaterial;
            sr.sprite = s_SharedExplosionSprite;
            sr.color = new Color(1f, 0.3f, 0.05f, 0.7f);
            explosion.transform.localScale = Vector3.one * (meteorImpactRadius * 1.8f);

            Destroy(explosion, 0.2f);
        }

        public override void TakeDamage(float damage, Vector2 knockback)
        {
            if (isDummyMode)
            {
                // Dummy Mode: hiển thị hiệu ứng trúng đòn nhưng giữ đầy máu để test combo
                base.TakeDamage(0f, knockback);
                currentHP = maxHP;
                UpdateHealthBar(0f, currentHP);
                return;
            }

            base.TakeDamage(damage, knockback);
        }

        // =====================================================================
        //  DEBUG & COMBAT SANDBOX INTEGRATION
        // =====================================================================

        /// <summary>
        /// Ép Boss tung chiêu Mưa Thiên Thạch ngay lập tức (phục vụ Sandbox Debug).
        /// </summary>
        public void ForceTriggerMeteorRain()
        {
            if (!isDead && !isCastingMeteor)
            {
                currentAttackCount = 0;
                StartCoroutine(MeteorRainUltimateCoroutine());
            }
        }

        /// <summary>
        /// Ép chuyển Boss sang Phase mong muốn (0, 1, 2) tức thì.
        /// </summary>
        public void ForceSetPhase(int targetPhase)
        {
            if (isDead) return;
            targetPhase = Mathf.Clamp(targetPhase, 0, TotalPhases - 1);

            if (targetPhase > 0 && targetPhase <= PhaseThresholds.Length)
            {
                currentHP = maxHP * (PhaseThresholds[targetPhase - 1] - 0.02f);
            }
            else
            {
                currentHP = maxHP;
            }

            UpdateHealthBar(0f, currentHP);
            Debug.Log($"[WorldBoss] [DebugSandbox] Đã ép Boss chuyển sang Phase {targetPhase}!");
        }

        /// <summary>
        /// Hồi đầy máu cho Boss.
        /// </summary>
        public void ResetBossHealth()
        {
            currentHP = maxHP;
            UpdateHealthBar(0f, currentHP);
        }

        /// <summary>
        /// Xử lý khi World Boss bị tiêu diệt: cộng thưởng tiến trình và kích hoạt màn hình tổng kết Chiến Thắng lượt chạy.
        /// </summary>
        protected override void HandleDeath()
        {
            base.HandleDeath();

            // Thưởng thêm vàng và ghi nhận tiến trình diệt World Boss
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData?.progressData != null)
            {
                SaveManager.Instance.CurrentSaveData.progressData.totalEnemiesKilled += 1;
                SaveManager.Instance.CurrentSaveData.progressData.totalCurrency += 500;
                SaveManager.Instance.SaveToDiskSync();
            }

            // Đợi 2s để animation chết chạy xong, sau đó chuyển trạng thái GameState.Victory để hiển thị UI tổng kết lượt chạy
            StartCoroutine(TriggerRunVictorySummaryCoroutine());
        }

        private IEnumerator TriggerRunVictorySummaryCoroutine()
        {
            yield return new WaitForSecondsRealtime(2.0f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Victory);
            }
        }

        // =====================================================================
        //  HEALTHBAR & PHASE INTEGRATION
        // =====================================================================

        private void SetupHealthBar()
        {
            // Kiểm tra nếu vô tình gắn component HealthBar lên chính GameObject của Boss
            if (healthBar != null && healthBar.gameObject == gameObject)
            {
                Debug.LogWarning("[WorldBoss] Cảnh báo: Component 'HealthBar' không được gắn trực tiếp lên GameObject của Boss! " +
                                 "HealthBar là một UI Prefab riêng biệt (Assets/Prefabs/Objects/BossHealthBar.prefab). " +
                                 "Vui lòng gỡ component HealthBar khỏi GameObject Boss và kéo Prefab BossHealthBar vào ô Health Bar.");
                healthBar = null;
            }

            // Prefab gán trực tiếp trong Inspector chưa có instance trong scene → phải Instantiate.
            if (healthBar != null && !healthBar.gameObject.scene.IsValid())
            {
                healthBar = Instantiate(healthBar);
                healthBar.name = "WorldBossHealthBar";
                ownsHealthBarInstance = true;
            }
            else if (healthBar == null)
            {
                // Tìm kiếm HealthBar UI hợp lệ trong Scene (loại trừ component nằm trên chính Boss)
                HealthBar[] allBars = FindObjectsOfType<HealthBar>(true);
                foreach (var bar in allBars)
                {
                    if (bar.gameObject != gameObject && bar.GetComponent<RectTransform>() != null)
                    {
                        healthBar = bar;
                        break;
                    }
                }
            }

            if (healthBar == null)
            {
                Debug.LogWarning("[WorldBoss] Không tìm thấy HealthBar hợp lệ. Bạn có thể kéo prefab 'Assets/Prefabs/Objects/BossHealthBar.prefab' vào field Health Bar trên Boss.");
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

        private void HandlePhaseChanged(int newPhase)
        {
            if (spriteRenderer == null) return;

            if (newPhase == 0)
            {
                if (originalMaterial != null) spriteRenderer.material = originalMaterial;
            }
            else
            {
                int groupIndex = Mathf.Clamp(newPhase, 0, phasePatterns.Count - 1);
                if (phasePatterns != null && groupIndex < phasePatterns.Count && phasePatterns[groupIndex] != null)
                {
                    Material phaseMaterial = phasePatterns[groupIndex].enragedMaterial;
                    if (phaseMaterial != null) spriteRenderer.material = phaseMaterial;
                }
            }

            Debug.Log($"[WorldBoss] Đã chuyển sang Phase {newPhase}! Tăng cường độ tấn công và thiên thạch.");
        }

        protected override void OnStateExit(EnemyState exitingState)
        {
            base.OnStateExit(exitingState);

            // Khi thoát khỏi Attack state, tắt toàn bộ hitbox ngay lập tức
            if (exitingState == EnemyState.Attack)
            {
                CleanupAttackHitboxes();
            }
        }

        protected override void OnStateEnter(EnemyState enteringState, EnemyState previousState)
        {
            base.OnStateEnter(enteringState, previousState);

            // Tắt hitbox và cancel coroutine khi bị hit hoặc chết
            if (enteringState == EnemyState.Hit || enteringState == EnemyState.Dead)
            {
                CleanupAttackHitboxes();

                if (attackLogicCoroutine != null)
                {
                    StopCoroutine(attackLogicCoroutine);
                    attackLogicCoroutine = null;
                }
            }

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
                        if (healthBar != null) healthBar.Hide();
                        if (spriteRenderer != null && originalMaterial != null) spriteRenderer.material = originalMaterial;
                        anim.SetTrigger("AI_die");
                        break;
                    case EnemyState.Hit:
                        anim.SetTrigger("AI_hit");
                        break;
                }
            }
        }

        /// <summary>
        /// Dọn dẹp và tắt triệt để các collider hitbox tấn công để tránh gây sát thương ngoài ý muốn.
        /// </summary>
        private void CleanupAttackHitboxes()
        {
            HitboxController hc = GetComponent<HitboxController>();
            if (hc != null)
            {
                hc.DeactivateHitboxes();
            }

            EntityHitboxHandler ehh = GetComponentInChildren<EntityHitboxHandler>();
            if (ehh != null)
            {
                ehh.StopAttack();
            }
        }

        // =====================================================================
        //  GIZMOS & DEBUG
        // =====================================================================

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Ground Check Circle (Xanh ngọc)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);

            // Meteor Spawn Area (Đỏ cam)
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, meteorSpawnRadius);

            // Meteor Impact Radius (Đỏ)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.right * 2f, meteorImpactRadius);

            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3.2f,
                $"WorldBoss (v{VERSION}) | Đòn đánh: {currentAttackCount}/{attacksBeforeMeteor} | Grounded: {isGrounded}"
            );
            #endif
        }

        // =====================================================================
        //  CONTEXT MENU – TEST NHANH TRONG UNITY INSPECTOR
        // =====================================================================

        [ContextMenu("Debug/Kích hoạt Mưa Thiên Thạch ngay lập tức")]
        private void DebugTriggerMeteorRain()
        {
            StartCoroutine(MeteorRainUltimateCoroutine());
        }

        [ContextMenu("Debug/Thực hiện cú nhảy thử nghiệm")]
        private void DebugPerformJump()
        {
            PerformJump(jumpForwardSpeed, jumpForce);
        }

        [ContextMenu("Debug/Gây 20% maxHP sát thương")]
        private void DebugTakeDamage()
        {
            float dmg = maxHP * 0.2f;
            TakeDamage(dmg);
            Debug.Log($"[WorldBoss] Debug: gây {dmg} sát thương ({currentHP}/{maxHP} HP)");
        }

        [ContextMenu("Debug/Hạ gục WorldBoss ngay")]
        private void DebugKillBoss()
        {
            TakeDamage(currentHP + 1f);
        }
    }
}
