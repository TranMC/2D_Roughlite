using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Roguelite.RoomSystem;

namespace Roguelite.Enemy
{
    public abstract class BossBase : EnemyBase
    {
        #region ====== PHASE SETTINGS ======

        [Header("===== Boss Phase Settings =====")]
        [Tooltip("Ngưỡng % máu để chuyển phase (giá trị từ 0.0 đến 1.0, sắp xếp GIẢM DẦN).\n" +
                 "VD: [0.7, 0.4] → Phase 1 khi HP <= 70%, Phase 2 khi HP <= 40%.")]
        [SerializeField] private float[] phaseThresholds = { 0.7f, 0.4f };

        /// <summary>Phase hiện tại (0 = phase mặc định, 1 = phase đầu tiên sau ngưỡng, ...).</summary>
        private int currentPhase = 0;

        /// <summary>Phase hiện tại (read-only cho bên ngoài).</summary>
        public int CurrentPhase => currentPhase;

        /// <summary>Tổng số phase (bao gồm phase 0 mặc định).</summary>
        public int TotalPhases => phaseThresholds.Length + 1;

        #endregion

        #region ====== ATTACK PATTERNS ======

        [System.Serializable]
        public class PhasePatternGroup
        {
            public string phaseName;
            [Tooltip("Hệ số tốc độ (Tăng cả tốc độ Animation VÀ tốc độ di chuyển thực, mặc định = 1)")]
            public float speedMultiplier = 1f;
            [Tooltip("Hệ số phóng to/thu nhỏ Boss cho Phase này (mặc định = 1)")]
            public float scaleMultiplier = 1f;
            [Tooltip("Material outline sẽ được apply khi boss ở phase này")]
            public Material enragedMaterial;
            public List<AttackPattern> patterns = new List<AttackPattern>();
        }

        [Header("===== Boss Attack Pattern Settings =====")]
        [SerializeField] protected List<PhasePatternGroup> phasePatterns = new List<PhasePatternGroup>();
        [SerializeField] private EntityHitboxHandler hitboxHandler;

        private AttackPattern activePattern;
        private bool isAttackingPattern = false;
        private Coroutine attackLockCoroutine;
        private Vector3 baseScale; // Lưu lại scale gốc để nhân với scaleMultiplier
        private float baseMoveSpeed; // Lưu lại tốc độ di chuyển gốc

        public AttackPattern ActivePattern => activePattern;
        public bool IsAttackingPattern => isAttackingPattern;

        #endregion

        #region ====== EVENTS ======

        /// <summary>
        /// Sự kiện khi Boss chuyển sang phase mới.
        /// Tham số: chỉ số phase mới (1, 2, 3...).
        /// Dùng để kích hoạt đổi pattern tấn công, VFX, tint màu Enrage, v.v.
        /// </summary>
        public event Action<int> OnPhaseChanged;

        #endregion

        // =====================================================================
        //  UNITY LIFECYCLE
        // =====================================================================

        protected override void Awake()
        {
            base.Awake();

            // Lưu lại scale và tốc độ di chuyển gốc ban đầu
            baseScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            baseMoveSpeed = moveSpeed;

            // Đăng ký lắng nghe event OnDamageTaken từ EnemyBase
            // để kiểm tra chuyển Phase sau mỗi lần nhận sát thương.
            // (TakeDamage là public non-virtual → không override được,
            //  nên dùng event hook thay thế)
            OnDamageTaken += HandlePhaseCheck;
        }

        protected override void Start()
        {
            base.Start();
            currentPhase = 0;

            // Validate phaseThresholds: phải sắp xếp giảm dần
            ValidateThresholds();

            // Áp dụng modifier cho Phase 0 ban đầu
            ApplyPhaseModifiers(currentPhase);
        }

        protected virtual void OnDestroy()
        {
            OnDamageTaken -= HandlePhaseCheck;
        }

        // =====================================================================
        //  OVERRIDDEN ENEMY BASE LOGIC
        // =====================================================================

        protected override void MoveHorizontal(float speed)
        {
            // Khóa hoàn toàn di chuyển khi đang ra chiêu theo pattern
            if (isAttackingPattern)
            {
                StopMovement();
                return;
            }
            base.MoveHorizontal(speed);
        }

        protected override void PerformAttack()
        {
            // Tránh kích hoạt đòn đánh mới khi đòn đánh trước chưa hết khóa di chuyển
            if (isAttackingPattern) return;

            AttackPattern pattern = GetRandomPatternForCurrentPhase();
            if (pattern != null)
            {
                TriggerAttackPattern(pattern);
            }
            else
            {
                base.PerformAttack();
            }
        }

        protected override void OnStateEnter(EnemyState enteringState, EnemyState previousState)
        {
            base.OnStateEnter(enteringState, previousState);

            // Bị trúng đòn khựng (stagger) -> Dừng đòn đánh hiện tại ngay lập tức
            if (enteringState == EnemyState.Hit)
            {
                StopActivePattern();
            }
        }

        // =====================================================================
        //  ATTACK PATTERN CONTROL LOGIC
        // =====================================================================

        public AttackPattern GetRandomPatternForCurrentPhase()
        {
            if (phasePatterns == null || phasePatterns.Count == 0) return null;

            int groupIndex = Mathf.Clamp(currentPhase, 0, phasePatterns.Count - 1);
            PhasePatternGroup group = phasePatterns[groupIndex];

            if (group != null && group.patterns != null && group.patterns.Count > 0)
            {
                int randIndex = UnityEngine.Random.Range(0, group.patterns.Count);
                return group.patterns[randIndex];
            }

            return null;
        }

        public void TriggerAttackPattern(AttackPattern pattern)
        {
            if (pattern == null) return;

            activePattern = pattern;
            isAttackingPattern = true;

            // Kích hoạt/Cấu hình hitbox
            if (hitboxHandler != null)
            {
                hitboxHandler.ExecuteAttack(pattern);
            }

            // Kích hoạt Animator Trigger tương ứng
            if (anim != null && !string.IsNullOrEmpty(pattern.AnimationTrigger))
            {
                anim.SetTrigger(pattern.AnimationTrigger);
            }

            // Chạy đếm ngược khóa di chuyển
            if (attackLockCoroutine != null)
            {
                StopCoroutine(attackLockCoroutine);
            }
            attackLockCoroutine = StartCoroutine(AttackLockCoroutine(pattern.AttackLockDuration));
        }

        private void StopActivePattern()
        {
            if (hitboxHandler != null)
            {
                hitboxHandler.StopAttack();
            }
            if (attackLockCoroutine != null)
            {
                StopCoroutine(attackLockCoroutine);
                attackLockCoroutine = null;
            }
            isAttackingPattern = false;
            activePattern = null;
        }

        private IEnumerator AttackLockCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            isAttackingPattern = false;
            activePattern = null;
            attackLockCoroutine = null;
        }

        // =====================================================================
        //  PHASE SYSTEM – Kiểm tra và chuyển Phase
        // =====================================================================

        /// <summary>
        /// Callback được gọi sau mỗi lần nhận sát thương (qua event OnDamageTaken).
        /// Tính % HP còn lại, nếu đạt ngưỡng phase tiếp theo → chuyển phase.
        /// </summary>
        /// <param name="damage">Lượng sát thương vừa nhận (không dùng trực tiếp).</param>
        /// <param name="remainingHP">HP còn lại sau khi trừ.</param>
        private void HandlePhaseCheck(float damage, float remainingHP)
        {
            if (currentPhase >= phaseThresholds.Length) return;

            float hpPercent = remainingHP / maxHP;

            // Duyệt qua các ngưỡng chưa đạt (hỗ trợ trường hợp đòn đánh mạnh
            // xuyên qua nhiều ngưỡng cùng lúc)
            while (currentPhase < phaseThresholds.Length
                   && hpPercent <= phaseThresholds[currentPhase])
            {
                currentPhase++;

                Debug.Log($"[BossBase] {gameObject.name} chuyển sang Phase {currentPhase}! " +
                          $"(HP: {hpPercent:P0})");

                ApplyPhaseModifiers(currentPhase);
                OnPhaseChanged?.Invoke(currentPhase);
            }
        }

        /// <summary>
        /// Cập nhật tốc độ Animator và Scale của Boss dựa theo cấu hình Phase hiện tại
        /// </summary>
        private void ApplyPhaseModifiers(int phaseIndex)
        {
            if (phasePatterns == null || phasePatterns.Count == 0) return;

            int groupIndex = Mathf.Clamp(phaseIndex, 0, phasePatterns.Count - 1);
            PhasePatternGroup group = phasePatterns[groupIndex];

            // Áp dụng tốc độ Animator
            if (anim != null)
            {
                anim.speed = group.speedMultiplier;
            }
            
            // Áp dụng tốc độ di chuyển thực tế (moveSpeed kế thừa từ EnemyBase)
            moveSpeed = baseMoveSpeed * group.speedMultiplier;

            // Áp dụng Scale (giữ nguyên hướng mặt hiện tại)
            float facingSign = Mathf.Sign(transform.localScale.x);
            transform.localScale = new Vector3(
                baseScale.x * group.scaleMultiplier * facingSign,
                baseScale.y * group.scaleMultiplier,
                baseScale.z * group.scaleMultiplier
            );
            
            Debug.Log($"[BossBase] Đã áp dụng Phase Modifiers: SpeedMultiplier={group.speedMultiplier}, Scale={group.scaleMultiplier}");
        }

        // =====================================================================
        //  DEATH – Override HandleDeath để mở cửa phòng Boss
        // =====================================================================

        /// <summary>
        /// Xử lý khi Boss chết: gọi logic gốc (EnemyBase.HandleDeath),
        /// sau đó tìm RoomManager của phòng Boss và gọi OnRoomCleared() để mở cửa.
        /// </summary>
        protected override void HandleDeath()
        {
            // Dừng đòn đánh hiện tại ngay khi chết
            StopActivePattern();

            base.HandleDeath();

            // Tìm RoomManager trên parent hoặc cùng hierarchy
            // (Boss thường nằm trong Room → RoomManager ở parent/root của Room)
            RoomManager roomManager = GetComponentInParent<RoomManager>();

            if (roomManager != null)
            {
                Debug.Log($"[BossBase] Boss {gameObject.name} đã bị hạ! Mở cửa phòng Boss.");
                roomManager.OnRoomCleared();
            }
            else
            {
                Debug.LogWarning($"[BossBase] Không tìm thấy RoomManager cho Boss {gameObject.name}.");
            }
        }

        // =====================================================================
        //  HELPERS
        // =====================================================================

        /// <summary>
        /// Kiểm tra mảng phaseThresholds có hợp lệ (giảm dần, trong khoảng 0-1).
        /// In cảnh báo nếu cấu hình sai.
        /// </summary>
        private void ValidateThresholds()
        {
            for (int i = 0; i < phaseThresholds.Length; i++)
            {
                if (phaseThresholds[i] < 0f || phaseThresholds[i] > 1f)
                {
                    Debug.LogWarning($"[BossBase] {gameObject.name}: phaseThresholds[{i}] = {phaseThresholds[i]} " +
                                     "nằm ngoài khoảng [0, 1]!");
                }

                if (i > 0 && phaseThresholds[i] >= phaseThresholds[i - 1])
                {
                    Debug.LogWarning($"[BossBase] {gameObject.name}: phaseThresholds phải sắp xếp GIẢM DẦN! " +
                                     $"[{i - 1}]={phaseThresholds[i - 1]}, [{i}]={phaseThresholds[i]}");
                }
            }
        }

        // =====================================================================
        //  GIZMOS – Hiển thị thông tin Phase trong Editor
        // =====================================================================

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Hiển thị phase hiện tại phía trên đầu Boss
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                $"Phase: {currentPhase}/{TotalPhases - 1}"
            );
            #endif
        }
    }
}
