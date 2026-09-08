using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

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
        public const string VERSION = "1.5.0";
        #region ====== SERIALIZE FIELDS ======

        [Header("===== Room Type Settings =====")]
        [Tooltip("Loại của căn phòng này (Start, Combat, Reward, Boss).")]
        public RoomType roomType = RoomType.Combat;

        [Header("===== Door Settings =====")]
        [Tooltip("Danh sách các RoomDoor của phòng. Tự động tìm kiếm ở các object con nếu để trống.")]
        [SerializeField] private RoomDoor[] roomDoors;

        [Header("===== Layer Settings =====")]
        [Tooltip("Layer dùng để nhận diện Player (phải trùng với Layer gán trên Player GameObject).")]
        [SerializeField] private LayerMask playerLayer;

        [Header("===== Spawner Settings =====")]
        [Tooltip("Tham chiếu tới EnemySpawner của phòng (Tự động tìm kiếm trên cùng GameObject nếu để trống).")]
        [SerializeField] private EnemySpawner enemySpawner;

        [Tooltip("Danh sách tất cả các EnemySpawner trong phòng (hỗ trợ nhiều spawner cho phòng đa quái / đa Boss).")]
        [SerializeField] private EnemySpawner[] enemySpawners;

        [Header("===== Space / Collision Settings =====")]
        [Tooltip("Collider đại diện cho kích thước vật lý của phòng để kiểm tra chồng lấn (Overlap Box).")]
        [SerializeField] private Collider2D roomBoundsCollider;

        [Header("===== Cinemachine Settings =====")]
        [Tooltip("Tham chiếu tới Cinemachine Virtual Camera riêng của phòng này (Phương án 2).")]
        [SerializeField] private CinemachineVirtualCamera roomVirtualCamera;

        [Tooltip("Tham chiếu tới Cinemachine Virtual Camera chính của Player (Phương án 1 - Fallback).")]
        [SerializeField] private CinemachineVirtualCamera playerVirtualCamera;

        [Tooltip("Khoảng cách thụt lùi (margin) từ lề phòng vào trong để kích hoạt camera & khóa phòng (tránh bị nhảy camera khi mới chạm mép cửa).")]
        [SerializeField] private float cameraTriggerMargin = 1.5f;

        [Tooltip("Độ ưu tiên VCam khi Player ở trong phòng.")]
        [SerializeField] private int activePriority = 20;

        [Tooltip("Độ ưu tiên VCam khi Player ngoài phòng.")]
        [SerializeField] private int inactivePriority = 0;

        #endregion

        #region ====== RUNTIME STATE ======

        /// <summary>
        /// Trạng thái phòng đã dọn sạch quái/Boss hay chưa.
        /// </summary>
        public bool IsCleared { get; private set; } = false;

        /// <summary>
        /// Cờ đảm bảo phòng chỉ bị khóa đúng 1 lần khi Player bước vào.
        /// </summary>
        private bool isRoomLocked = false;

        /// <summary>
        /// Cache Collider2D nhận diện Player của RoomManager.
        /// </summary>
        private Collider2D triggerCollider;

        /// <summary>
        /// Cache component CinemachineConfiner2D để giới hạn phạm vi camera (Fallback Phương án 1).
        /// </summary>
        private CinemachineConfiner2D cameraConfiner;

        #endregion

        // =====================================================================
        //  UNITY LIFECYCLE
        // =====================================================================

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();

            // Đảm bảo Collider là trigger để phát hiện va chạm không cản trở vật lý
            triggerCollider.isTrigger = true;

            // Tự động tìm kiếm các RoomDoor con nếu chưa được gán trong Inspector
            if (roomDoors == null || roomDoors.Length == 0)
            {
                roomDoors = GetComponentsInChildren<RoomDoor>();
            }

            // Tự động gán roomBoundsCollider nếu chưa gán
            if (roomBoundsCollider == null)
            {
                roomBoundsCollider = GetComponent<Collider2D>();
            }

            // Tự động tìm kiếm và đồng bộ tất cả EnemySpawner trong phòng
            InitEnemySpawners();

            // Tự động tìm kiếm Cinemachine Virtual Camera trong phòng (Phương án 2)
            if (roomVirtualCamera == null)
            {
                roomVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            }

            if (roomVirtualCamera != null)
            {
                roomVirtualCamera.Priority = inactivePriority;
                CinemachineConfiner2D confiner = roomVirtualCamera.GetComponent<CinemachineConfiner2D>();
                if (confiner != null && roomBoundsCollider != null && confiner.m_BoundingShape2D == null)
                {
                    confiner.m_BoundingShape2D = roomBoundsCollider;
                }
            }
            else if (playerVirtualCamera == null)
            {
                // Fallback Phương án 1: Tự động tìm camera chính trên Scene
                playerVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            }

            // Cache Cinemachine Confiner từ Virtual Camera chung (nếu dùng phương án 1)
            if (playerVirtualCamera != null)
            {
                cameraConfiner = playerVirtualCamera.GetComponent<CinemachineConfiner2D>();
            }
        }

        private void Start()
        {
            // Nếu là Start Room, ưu tiên tự động kích hoạt camera ngay khi màn chơi vừa khởi tạo xong
            if (roomType == RoomType.Start)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    ActivateRoomCamera(player.transform);
                }
                else
                {
                    GameObject playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                    {
                        ActivateRoomCamera(playerObj.transform);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
            if (enemySpawners != null)
            {
                foreach (var s in enemySpawners)
                {
                    if (s != null)
                    {
                        s.OnAllEnemiesCleared -= CheckAndTriggerRoomClear;
                    }
                }
            }

            if (enemySpawner != null)
            {
                enemySpawner.OnAllEnemiesCleared -= OnRoomCleared;
                enemySpawner.OnAllEnemiesCleared -= CheckAndTriggerRoomClear;
            }
        }

        // =====================================================================
        //  [BƯỚC 1] PLAYER ENTER ROOM – Nhận diện Player bằng Layer & Inner Bounds
        // =====================================================================

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryHandlePlayerEntry(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryHandlePlayerEntry(collision);
        }

        private void TryHandlePlayerEntry(Collider2D collision)
        {
            // Kiểm tra Layer của đối tượng va chạm có nằm trong playerLayer không
            if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

            // Kiểm tra người chơi đã thực sự bước hẳn qua mép cửa vào trong phòng chưa
            if (!IsPlayerInsideRoomInnerBounds(collision.transform.position)) return;

            // Chỉ kích hoạt khi VCam chưa ở độ ưu tiên cao nhất
            if (roomVirtualCamera != null && roomVirtualCamera.Priority != activePriority)
            {
                ActivateRoomCamera(collision.transform);
            }

            // Khóa phòng và sinh quái nếu phòng chưa bị khóa
            if (!isRoomLocked && roomType != RoomType.Start)
            {
                Debug.Log($"[RoomManager] Player đã thực sự bước vào trong phòng: {gameObject.name}");
                LockRoom();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // Kiểm tra Layer của đối tượng va chạm
            if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

            // Hạ độ ưu tiên VCam của phòng này khi Player rời đi hẳn khỏi phòng
            DeactivateRoomCamera();
        }

        /// <summary>
        /// Kiểm tra xem vị trí của Player có nằm sâu bên trong vùng nội bộ của phòng (vượt qua mép cửa) hay không.
        /// </summary>
        public bool IsPlayerInsideRoomInnerBounds(Vector3 playerPos)
        {
            if (roomBoundsCollider == null) return true;

            Bounds bounds = roomBoundsCollider.bounds;

            // Thụt lùi ranh giới vào bên trong theo cameraTriggerMargin (tính bằng Tile/Đơn vị Unity)
            float marginX = Mathf.Min(cameraTriggerMargin, bounds.extents.x * 0.4f);
            float marginY = Mathf.Min(cameraTriggerMargin, bounds.extents.y * 0.4f);

            Vector3 min = bounds.min + new Vector3(marginX, marginY, 0);
            Vector3 max = bounds.max - new Vector3(marginX, marginY, 0);

            return playerPos.x >= min.x && playerPos.x <= max.x &&
                   playerPos.y >= min.y && playerPos.y <= max.y;
        }

        /// <summary>
        /// Kích hoạt camera riêng của phòng khi Player bước vào.
        /// </summary>
        public void ActivateRoomCamera(Transform playerTransform)
        {
            if (roomVirtualCamera != null)
            {
                if (roomVirtualCamera.Follow == null || roomVirtualCamera.Follow != playerTransform)
                {
                    roomVirtualCamera.Follow = playerTransform;
                }
                roomVirtualCamera.Priority = activePriority;
            }
        }

        /// <summary>
        /// Hạ độ ưu tiên camera của phòng khi Player rời khỏi.
        /// </summary>
        public void DeactivateRoomCamera()
        {
            if (roomVirtualCamera != null)
            {
                roomVirtualCamera.Priority = inactivePriority;
            }
        }

        // Vẽ đường giới hạn vùng cảm biến kích hoạt camera trong Editor (chọn RoomManager để thấy)
        private void OnDrawGizmosSelected()
        {
            if (roomBoundsCollider != null)
            {
                Bounds bounds = roomBoundsCollider.bounds;
                float marginX = Mathf.Min(cameraTriggerMargin, bounds.extents.x * 0.4f);
                float marginY = Mathf.Min(cameraTriggerMargin, bounds.extents.y * 0.4f);

                Vector3 min = bounds.min + new Vector3(marginX, marginY, 0);
                Vector3 max = bounds.max - new Vector3(marginX, marginY, 0);
                Vector3 center = (min + max) * 0.5f;
                Vector3 size = max - min;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(center, size);
            }
        }

        // =====================================================================
        //  [BƯỚC 2] LOCK DOORS – Khóa tất cả các cửa có kết nối
        // =====================================================================

        /// <summary>
        /// Khóa phòng: đánh dấu cờ, bật các cửa chặn lối đi, sau đó gọi SpawnEnemies.
        /// </summary>
        private void LockRoom()
        {
            // Đánh dấu phòng đã khóa
            isRoomLocked = true;

            // Khóa toàn bộ các cửa đang có kết nối hoạt động
            if (roomDoors != null)
            {
                for (int i = 0; i < roomDoors.Length; i++)
                {
                    if (roomDoors[i] != null)
                    {
                        roomDoors[i].SetGateActive(true);
                    }
                }
            }

            // Khóa camera vào ranh giới phòng (Fallback cho Phương án 1)
            if (cameraConfiner != null && roomBoundsCollider != null)
            {
                cameraConfiner.m_BoundingShape2D = roomBoundsCollider;
                cameraConfiner.InvalidateCache();
            }

            Debug.Log($"[RoomManager] Phòng {gameObject.name} đã bị khóa! Cửa chặn đã đóng để chiến đấu.");

            // [BƯỚC 3] Chuyển tiếp sang sinh quái
            SpawnEnemies();
        }

        // =====================================================================
        //  [BƯỚC 3] SPAWN ENEMIES – Sinh quái
        // =====================================================================

        /// <summary>
        /// Sinh quái vật trong phòng thông qua EnemySpawner.
        /// </summary>
        private void SpawnEnemies()
        {
            if (enemySpawners != null && enemySpawners.Length > 0)
            {
                Debug.Log($"[RoomManager] Phòng {gameObject.name} bị khóa, kích hoạt {enemySpawners.Length} EnemySpawner sinh quái...");
                foreach (var s in enemySpawners)
                {
                    if (s != null)
                    {
                        s.SpawnEnemies();
                    }
                }
            }
            else if (enemySpawner != null)
            {
                Debug.Log($"[RoomManager] Phòng {gameObject.name} bị khóa, yêu cầu EnemySpawner sinh quái...");
                enemySpawner.SpawnEnemies();
            }
            else
            {
                if (CheckIfAllEnemiesDefeated())
                {
                    Debug.LogWarning($"[RoomManager] Không tìm thấy EnemySpawner và không có quái vật trong phòng {gameObject.name}! Tự động hoàn thành.");
                    OnRoomCleared();
                }
                else
                {
                    Debug.Log($"[RoomManager] Phòng {gameObject.name} không có Spawner nhưng có quái vật đặt sẵn. Chờ người chơi tiêu diệt sạch...");
                }
            }
        }

        // =====================================================================
        //  [BƯỚC 4-5-6-7] ROOM CLEARED → REWARD → OPEN DOORS
        // =====================================================================

        /// <summary>
        /// Kiểm tra xem toàn bộ quái vật và Boss trong phòng đã bị tiêu diệt sạch chưa.
        /// </summary>
        public bool CheckIfAllEnemiesDefeated()
        {
            // 1. Kiểm tra tất cả các Spawner trong phòng xem còn quái sống không
            if (enemySpawners != null)
            {
                foreach (var spawner in enemySpawners)
                {
                    if (spawner != null && spawner.HasActiveEnemies)
                    {
                        Debug.Log($"[RoomManager] Spawner '{spawner.gameObject.name}' vẫn còn {spawner.ActiveEnemyCount} quái vật sống.");
                        return false;
                    }
                }
            }

            // 2. Quét tất cả EnemyBase (bao gồm BossBase) con trong phòng
            Roguelite.Enemy.EnemyBase[] allEnemies = GetComponentsInChildren<Roguelite.Enemy.EnemyBase>(true);
            foreach (var enemy in allEnemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead && enemy.CurrentHP > 0f)
                {
                    Debug.Log($"[RoomManager] Mục tiêu '{enemy.gameObject.name}' (HP: {enemy.CurrentHP}) vẫn còn sống trong phòng '{gameObject.name}'.");
                    return false;
                }
            }

            return true;
        }

        private Coroutine delayedClearCheckCoroutine;

        /// <summary>
        /// Kiểm tra an toàn trước khi quyết định phòng đã được dọn sạch.
        /// Chỉ khi toàn bộ quái và Boss trong phòng đã chết thì mới gọi OnRoomCleared().
        /// </summary>
        public void CheckAndTriggerRoomClear()
        {
            if (IsCleared) return;

            if (!CheckIfAllEnemiesDefeated())
            {
                Debug.Log($"[RoomManager] Nhận thông báo một quái/Boss trong phòng '{gameObject.name}' bị hạ, nhưng phòng vẫn còn quái/Boss khác còn sống. Chưa clear phòng!");

                // Lên lịch kiểm tra lại sau một nhịp ngắn (đề phòng nhiều quái cùng chết trong một frame hoặc đòn đánh diện rộng)
                if (delayedClearCheckCoroutine != null)
                {
                    StopCoroutine(delayedClearCheckCoroutine);
                }
                delayedClearCheckCoroutine = StartCoroutine(DelayedClearCheckRoutine());
                return;
            }

            if (delayedClearCheckCoroutine != null)
            {
                StopCoroutine(delayedClearCheckCoroutine);
                delayedClearCheckCoroutine = null;
            }

            Debug.Log($"[RoomManager] Toàn bộ quái vật và Boss trong phòng '{gameObject.name}' đã bị tiêu diệt sạch! Tiến hành Clear phòng...");
            OnRoomCleared();
        }

        private IEnumerator DelayedClearCheckRoutine()
        {
            yield return new WaitForSeconds(0.25f);
            if (!IsCleared && CheckIfAllEnemiesDefeated())
            {
                Debug.Log($"[RoomManager] [DelayedCheck] Xác nhận toàn bộ quái và Boss trong phòng '{gameObject.name}' đã dọn sạch! Tiến hành Clear phòng...");
                OnRoomCleared();
            }
            delayedClearCheckCoroutine = null;
        }

        /// <summary>
        /// Khởi tạo và liên kết toàn bộ EnemySpawner trong phòng.
        /// </summary>
        private void InitEnemySpawners()
        {
            var foundSpawners = new System.Collections.Generic.List<EnemySpawner>();
            if (enemySpawners != null && enemySpawners.Length > 0)
            {
                foreach (var s in enemySpawners)
                {
                    if (s != null && !foundSpawners.Contains(s)) foundSpawners.Add(s);
                }
            }

            if (enemySpawner != null && !foundSpawners.Contains(enemySpawner))
            {
                foundSpawners.Add(enemySpawner);
            }

            EnemySpawner[] childSpawners = GetComponentsInChildren<EnemySpawner>(true);
            foreach (var s in childSpawners)
            {
                if (s != null && !foundSpawners.Contains(s)) foundSpawners.Add(s);
            }

            enemySpawners = foundSpawners.ToArray();
            if (enemySpawner == null && enemySpawners.Length > 0)
            {
                enemySpawner = enemySpawners[0];
            }

            foreach (var s in enemySpawners)
            {
                if (s != null)
                {
                    s.OnAllEnemiesCleared -= CheckAndTriggerRoomClear;
                    s.OnAllEnemiesCleared += CheckAndTriggerRoomClear;
                }
            }
        }

        /// <summary>
        /// Được gọi từ bên ngoài khi tất cả quái trong phòng đã bị tiêu diệt.
        /// Thực hiện chuỗi: Room Cleared → Reward/Upgrade → Open Doors.
        /// </summary>
        public void OnRoomCleared()
        {
            if (IsCleared)
            {
                Debug.LogWarning($"[RoomManager] Phòng {gameObject.name} đã được dọn sạch trước đó, bỏ qua yêu cầu trùng lặp.");
                return;
            }

            IsCleared = true;
            Debug.Log($"[RoomManager] Phòng {gameObject.name} (Loại: {roomType}) đã được dọn sạch hoàn toàn!");

            // Kiểm tra xem phòng này có phải là phòng chứa World Boss không
            bool isWorldBossEncounter = GetComponentInChildren<Roguelite.Enemy.WorldBoss>(true) != null;

            if (isWorldBossEncounter)
            {
                // World Boss là trực tiếp kết thúc trận, không nhận Perk
                Debug.Log("[RoomManager] Đã dọn phòng World Boss! Trực tiếp chuyển trạng thái kết thúc, không trao Perk.");
            }
            else if (roomType != RoomType.Start)
            {
                // [BƯỚC 6] Reward/Upgrade - Các phòng chiến đấu (Combat, Boss, Reward) khi dọn sạch đều trao thưởng Perk chọn 1 trong 3
                Debug.Log($"[RoomManager] Đã dọn sạch phòng {roomType}! Trao thưởng Perk chọn 1 trong 3 cho người chơi...");
                if (Roguelite.UI.RewardSelectionController.Instance != null)
                {
                    Roguelite.UI.RewardSelectionController.Instance.OpenSelection();
                }
                else
                {
                    Debug.LogWarning("[RoomManager] Không tìm thấy RewardSelectionController Instance trong Scene!");
                }
            }
            else
            {
                Debug.Log($"[RoomManager] Phòng {roomType} không yêu cầu chiến đấu, không trao thưởng Perk.");
            }

            // [BƯỚC 7] Open Doors – Mở các cửa chặn để mở lối đi tiếp
            OpenDoors();
        }

        /// <summary>
        /// Mở các cửa chặn bằng cách vô hiệu hóa gateObject.
        /// </summary>
        private void OpenDoors()
        {
            if (roomDoors != null)
            {
                for (int i = 0; i < roomDoors.Length; i++)
                {
                    if (roomDoors[i] != null)
                    {
                        roomDoors[i].SetGateActive(false);
                    }
                }
            }

            // Giải phóng camera khi phòng đã mở
            if (cameraConfiner != null)
            {
                cameraConfiner.m_BoundingShape2D = null;
                cameraConfiner.InvalidateCache();
            }

            Debug.Log($"[RoomManager] Tất cả cửa phòng {gameObject.name} đã mở!");
        }

        // =====================================================================
        //  PUBLIC HELPER METHODS FOR GENERATOR
        // =====================================================================

        /// <summary>
        /// Lấy toàn bộ RoomDoor trong phòng này.
        /// </summary>
        public RoomDoor[] GetDoors()
        {
            if (roomDoors == null || roomDoors.Length == 0)
            {
                roomDoors = GetComponentsInChildren<RoomDoor>();
            }
            return roomDoors;
        }

        /// <summary>
        /// Lấy vùng bao (Bounds) vật lý của phòng để kiểm tra đè lấn.
        /// </summary>
        public Bounds GetRoomBounds()
        {
            if (roomBoundsCollider != null)
            {
                return roomBoundsCollider.bounds;
            }
            // Fallback nếu không gán
            if (triggerCollider != null)
            {
                return triggerCollider.bounds;
            }
            return new Bounds(transform.position, Vector3.zero);
        }
    }
}
