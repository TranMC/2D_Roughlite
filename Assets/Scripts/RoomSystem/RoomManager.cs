using UnityEngine;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Quản lý luồng hoạt động của một căn phòng theo Flowchart:
    /// [1] Player Enter Room → [2] Lock Doors → [3] Spawn Enemies
    /// → [4] Check Enemies Alive? → [5] Room Cleared → [6] Reward/Upgrade → [7] Open Doors
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RoomManager : MonoBehaviour
    {
        #region ====== SERIALIZE FIELDS ======

        [Header("===== Door Settings =====")]
        [Tooltip("Mảng các GameObject cửa. Khi khóa phòng: SetActive(true) để chặn lối đi. Khi mở: SetActive(false).")]
        [SerializeField] private GameObject[] doors;

        [Header("===== Layer Settings =====")]
        [Tooltip("Layer dùng để nhận diện Player (phải trùng với Layer gán trên Player GameObject).")]
        [SerializeField] private LayerMask playerLayer;

        [Header("===== Spawner Settings =====")]
        [Tooltip("Tham chiếu tới EnemySpawner của phòng (Tự động tìm kiếm trên cùng GameObject nếu để trống).")]
        [SerializeField] private EnemySpawner enemySpawner;

        #endregion

        #region ====== RUNTIME STATE ======

        /// <summary>
        /// Cờ đảm bảo phòng chỉ bị khóa đúng 1 lần khi Player bước vào.
        /// </summary>
        private bool isRoomLocked = false;

        /// <summary>
        /// Cache Collider2D nhận diện Player của RoomManager (tắt sau khi kích hoạt).
        /// </summary>
        private Collider2D triggerCollider;

        #endregion

        // =====================================================================
        //  UNITY LIFECYCLE
        // =====================================================================

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();

            // Đảm bảo Collider là trigger để phát hiện va chạm không cản trở vật lý
            triggerCollider.isTrigger = true;

            // Tự động tìm kiếm EnemySpawner trên cùng GameObject nếu chưa gán
            if (enemySpawner == null)
            {
                enemySpawner = GetComponent<EnemySpawner>();
            }

            // Đăng ký sự kiện hoàn thành dọn phòng
            if (enemySpawner != null)
            {
                enemySpawner.OnAllEnemiesCleared += OnRoomCleared;
            }
        }

        private void OnDestroy()
        {
            // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
            if (enemySpawner != null)
            {
                enemySpawner.OnAllEnemiesCleared -= OnRoomCleared;
            }
        }

        // =====================================================================
        //  [BƯỚC 1] PLAYER ENTER ROOM – Nhận diện Player bằng Layer
        // =====================================================================

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Bỏ qua nếu phòng đã bị khóa (tránh kích hoạt lần 2)
            if (isRoomLocked) return;

            // Kiểm tra Layer của đối tượng va chạm có nằm trong playerLayer không
            // Dùng bitwise shift để so sánh đúng chuẩn LayerMask
            if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

            // Player đã bước vào phòng → Kích hoạt chuỗi sự kiện
            Debug.Log($"[RoomManager] Player đã bước vào phòng: {gameObject.name}");
            LockRoom();
        }

        // =====================================================================
        //  [BƯỚC 2] LOCK DOORS – Khóa tất cả các cửa
        // =====================================================================

        /// <summary>
        /// Khóa phòng: đánh dấu cờ, bật các cửa chặn lối đi,
        /// tắt Collider nhận diện để tối ưu hiệu suất, sau đó gọi SpawnEnemies.
        /// </summary>
        private void LockRoom()
        {
            // Đánh dấu phòng đã khóa
            isRoomLocked = true;

            // Bật tất cả các cửa lên để chặn lối đi
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null)
                {
                    doors[i].SetActive(true);
                }
            }

            // Tắt Collider nhận diện của RoomManager – không cần quét nữa
            triggerCollider.enabled = false;

            Debug.Log($"[RoomManager] Phòng {gameObject.name} đã bị khóa! Tổng cộng {doors.Length} cửa đã đóng.");

            // [BƯỚC 3] Chuyển tiếp sang sinh quái
            SpawnEnemies();
        }

        // =====================================================================
        //  [BƯỚC 3] SPAWN ENEMIES – Sinh quái (đầu chờ cho Task sau)
        // =====================================================================

        /// <summary>
        /// Sinh quái vật trong phòng thông qua EnemySpawner.
        /// </summary>
        private void SpawnEnemies()
        {
            if (enemySpawner != null)
            {
                Debug.Log($"[RoomManager] Phòng {gameObject.name} bị khóa, yêu cầu EnemySpawner sinh quái...");
                enemySpawner.SpawnEnemies();
            }
            else
            {
                Debug.LogWarning($"[RoomManager] Không tìm thấy EnemySpawner cho phòng {gameObject.name}! Tự động hoàn thành.");
                OnRoomCleared();
            }
        }

        // =====================================================================
        //  [BƯỚC 4-5-6-7] ROOM CLEARED → REWARD → OPEN DOORS
        // =====================================================================

        /// <summary>
        /// Được gọi từ bên ngoài khi tất cả quái trong phòng đã bị tiêu diệt.
        /// Thực hiện chuỗi: Room Cleared → Reward/Upgrade → Open Doors.
        /// </summary>
        public void OnRoomCleared()
        {
            // [BƯỚC 5] Room Cleared
            Debug.Log($"[RoomManager] Phòng {gameObject.name} đã được dọn sạch!");

            // [BƯỚC 6] Reward/Upgrade (đầu chờ cho Task sau)
            // TODO: Ghép nối với hệ thống Reward/Upgrade
            Debug.Log($"[RoomManager] Chuẩn bị trao thưởng...");

            // [BƯỚC 7] Open Doors – Tắt các cửa để mở lối đi
            OpenDoors();
        }

        /// <summary>
        /// Mở tất cả các cửa bằng cách tắt GameObject.
        /// </summary>
        private void OpenDoors()
        {
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] != null)
                {
                    doors[i].SetActive(false);
                }
            }

            Debug.Log($"[RoomManager] Tất cả cửa phòng {gameObject.name} đã mở!");
        }
    }
}
