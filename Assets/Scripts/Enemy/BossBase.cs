using System;
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
        }

        protected virtual void OnDestroy()
        {
            OnDamageTaken -= HandlePhaseCheck;
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

                OnPhaseChanged?.Invoke(currentPhase);
            }
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
