using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Roguelite.Combat;
using Roguelite.Core;
using Roguelite.RoomSystem;

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
        public const string VERSION = "1.3.0";

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

        private bool ownsHealthBarInstance = false;
        private Material originalMaterial;

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

            // 2. Nếu đang rượt đuổi (Chase) và mục tiêu đã trong tầm đánh -> Chuyển sang Attack ngay khi chạm đất
            if (CurrentState == EnemyState.Chase && playerTarget != null && IsTargetAlive())
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
                {
                    TransitionToState(EnemyState.Attack);
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
        /// Thực hiện cú nhảy để vượt tường, vượt vực hoặc nhảy lên bục cao.
        /// </summary>
        public void PerformJump(float forwardSpeed, float verticalForce)
        {
            if (rb == null || !isGrounded || jumpCooldownTimer > 0f) return;

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
        //  CHASE LOGIC OVERRIDE (TÍCH HỢP NHẢY KHI TRUY ĐUỔI)
        // =====================================================================

        protected override void ChaseLogic()
        {
            if (playerTarget == null || !IsTargetAlive())
            {
                playerTarget = null;
                if (isGrounded)
                {
                    TransitionToState(EnemyState.Idle);
                }
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            // Player ra ngoài tầm phát hiện
            if (distanceToPlayer > detectionRange)
            {
                playerTarget = null;
                if (isGrounded)
                {
                    TransitionToState(EnemyState.Idle);
                }
                return;
            }

            // Quay mặt về phía Player
            FaceTarget(playerTarget.position);

            // Trong tầm đánh cận chiến (chỉ tấn công khi đã tiếp đất)
            if (distanceToPlayer <= attackRange)
            {
                if (isGrounded)
                {
                    if (attackCooldownTimer <= 0f)
                    {
                        TransitionToState(EnemyState.Attack);
                    }
                    else
                    {
                        StopMovement();
                    }
                    return;
                }
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
                PerformJump(jumpForwardSpeed, jumpForce);
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

        protected override void PatrolLogic()
        {
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
                TransitionToState(EnemyState.Chase);
            }
        }

        // =====================================================================
        //  ATTACK SYSTEM & METEOR RAIN ULTIMATE (ĐẾM 4 ĐÒN ĐÁNH)
        // =====================================================================

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

            // Tính toán số lượng thiên thạch theo Phase hiện tại
            int totalMeteors = baseMeteorCount + (CurrentPhase * 2);

            // Xác định tâm dội thiên thạch (ưu tiên vị trí Player)
            Vector3 centerTargetPos = playerTarget != null ? playerTarget.position : (transform.position + Vector3.right * facingDirection * 3f);

            // 1. Tạo danh sách các điểm rơi thiên thạch
            List<Vector3> impactPoints = new List<Vector3>();
            impactPoints.Add(centerTargetPos); // Quả đầu tiên luôn nhắm thẳng vào Player

            for (int i = 1; i < totalMeteors; i++)
            {
                float randomOffsetX = UnityEngine.Random.Range(-meteorSpawnRadius, meteorSpawnRadius);
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

            // 3. Cho các quả thiên thạch dội xuống
            for (int i = 0; i < impactPoints.Count; i++)
            {
                Vector3 impactPos = impactPoints[i];
                GameObject marker = i < telegraphObjects.Count ? telegraphObjects[i] : null;

                StartCoroutine(DropSingleMeteor(impactPos, marker));

                // Giãn cách một chút giữa các quả thiên thạch liên tiếp để tạo cảm giác mưa dội
                yield return new WaitForSeconds(0.12f);
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
            float spawnOffsetX = UnityEngine.Random.Range(-3f, 3f);
            Vector3 startPos = targetImpactPos + new Vector3(spawnOffsetX, spawnHeight, 0f);

            GameObject meteorInstance = null;

            if (meteorPrefab != null)
            {
                meteorInstance = Instantiate(meteorPrefab, startPos, Quaternion.identity);
            }
            else
            {
                // Fallback Procedural Meteor
                meteorInstance = CreateProceduralMeteorObject(startPos);
            }

            // Tính hướng rơi
            Vector3 travelDir = (targetImpactPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, targetImpactPos);
            float duration = distance / meteorFallSpeed;
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

            // HitStop nhẹ tạo lực chấn động
            if (HitStopManager.Instance != null)
            {
                HitStopManager.Instance.LightHitStop();
            }

            // Tìm và gây sát thương cho các đối tượng trong vùng nổ
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(impactPos, meteorImpactRadius, playerLayer);

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
        }

        // =====================================================================
        //  PROCEDURAL FALLBACK VISUALS (TỰ ĐỘNG SINH VFX KHI KHÔNG CÓ PREFAB)
        // =====================================================================

        private GameObject SpawnTelegraphMarker(Vector3 position)
        {
            if (telegraphMarkerPrefab != null)
            {
                return Instantiate(telegraphMarkerPrefab, position, Quaternion.identity);
            }

            // Fallback: Tạo vòng tròn đỏ cảnh báo bằng GameObject + LineRenderer
            GameObject marker = new GameObject("TelegraphMarker_Procedural");
            marker.transform.position = position;

            LineRenderer line = marker.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(1f, 0.2f, 0.2f, 0.8f);
            line.endColor = new Color(1f, 0.4f, 0.1f, 0.8f);

            int segments = 24;
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
            GameObject meteor = new GameObject("Meteor_Procedural");
            meteor.transform.position = position;

            SpriteRenderer sr = meteor.AddComponent<SpriteRenderer>();
            sr.material = new Material(Shader.Find("Sprites/Default"));
            sr.color = new Color(1f, 0.45f, 0.1f, 1f);

            // Tạo Sprite hình tròn cho thiên thạch
            Texture2D tex = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                    tex.SetPixel(x, y, dist <= 14f ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            meteor.transform.localScale = Vector3.one * 1.2f;

            // Trail Renderer tạo đuôi lửa
            TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.8f;
            trail.endWidth = 0.1f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 0.6f, 0.1f, 0.9f);
            trail.endColor = new Color(1f, 0.1f, 0f, 0f);

            return meteor;
        }

        private void CreateProceduralExplosionEffect(Vector3 position)
        {
            GameObject explosion = new GameObject("Explosion_Procedural");
            explosion.transform.position = position;

            SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
            sr.material = new Material(Shader.Find("Sprites/Default"));
            sr.color = new Color(1f, 0.3f, 0.05f, 0.9f);

            Texture2D tex = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                    tex.SetPixel(x, y, dist <= 14f ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            explosion.transform.localScale = Vector3.one * (meteorImpactRadius * 2f);

            // Tự hủy sau 0.25s
            Destroy(explosion, 0.25f);
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

        protected override void OnStateEnter(EnemyState enteringState, EnemyState previousState)
        {
            base.OnStateEnter(enteringState, previousState);

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
