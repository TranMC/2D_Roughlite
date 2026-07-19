using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Thuật toán sinh màn chơi procedural bằng cách ghép nối cửa (Doorway Alignment) không chồng lấn.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("===== Room Prefabs =====")]
        [Tooltip("Prefab của phòng bắt đầu (Start Room).")]
        [SerializeField] private GameObject startRoomPrefab;

        [Tooltip("Danh sách prefab của các phòng chiến đấu (Combat Room).")]
        [SerializeField] private GameObject[] combatRoomPrefabs;

        [Tooltip("Prefab của phòng phần thưởng (Reward Room).")]
        [SerializeField] private GameObject rewardRoomPrefab;

        [Tooltip("Prefab của phòng trùm (Boss Room).")]
        [SerializeField] private GameObject bossRoomPrefab;

        [Header("===== Generation Config =====")]
        [Tooltip("Số lượng phòng tối đa trong màn chơi này (bao gồm Start, Reward, Boss).")]
        [SerializeField] private int totalRooms = 8;

        [Tooltip("Layer gán trên các phòng để kiểm tra chồng lấn vật lý (Overlap).")]
        [SerializeField] private LayerMask roomLayer;

        [Tooltip("Số lượt thử tối đa trước khi quyết định sinh lại toàn bộ bản đồ.")]
        [SerializeField] private int maxGlobalAttempts = 20;

        // Lưu trữ các phòng đã sinh để dọn dẹp hoặc truy xuất
        private List<RoomManager> spawnedRooms = new List<RoomManager>();

        private void Start()
        {
            if (Application.isPlaying)
            {
                GenerateMapWithRetry();
            }
        }

        [ContextMenu("Generate Map (Editor)")]
        public void GenerateMapEditor()
        {
            GenerateMapWithRetry();
        }

        [ContextMenu("Clear Map (Editor)")]
        public void ClearMapEditor()
        {
            ClearPreviousMap();
        }

        [ContextMenu("Debug/Dịch chuyển tới phòng Boss")]
        public void TeleportPlayerToBossRoom()
        {
            if (spawnedRooms == null || spawnedRooms.Count == 0)
            {
                Debug.LogWarning("[MapGenerator] Chưa có bản đồ được sinh!");
                return;
            }

            RoomManager bossRoom = spawnedRooms.Find(r => r.roomType == RoomType.Boss);
            if (bossRoom == null)
            {
                Debug.LogWarning("[MapGenerator] Không tìm thấy phòng Boss trong danh sách phòng đã sinh!");
                return;
            }

            // Tìm Player
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                // Dịch chuyển người chơi đến tâm phòng Boss
                player.transform.position = bossRoom.transform.position;
                Debug.Log($"[MapGenerator] Đã dịch chuyển Player tới phòng Boss tại: {bossRoom.transform.position}");
            }
            else
            {
                Debug.LogWarning("[MapGenerator] Không tìm thấy Player trong Scene!");
            }
        }

        /// <summary>
        /// Sinh bản đồ với cơ chế thử lại nếu bị kẹt (không tìm thấy không gian trống).
        /// </summary>
        public void GenerateMapWithRetry()
        {
            int attempts = 0;
            bool success = false;

            while (!success && attempts < maxGlobalAttempts)
            {
                attempts++;
                ClearPreviousMap();
                success = TryGenerateMap();

                if (success)
                {
                    Debug.Log($"[MapGenerator] Sinh bản đồ thành công sau {attempts} lượt thử!");
                }
            }

            if (!success)
            {
                Debug.LogError("[MapGenerator] Vượt quá số lượt thử tối đa! Không thể sinh bản đồ hợp lệ. Vui lòng kiểm tra lại cấu hình prefab.");
            }
        }

        /// <summary>
        /// Xóa các phòng đã sinh trước đó (dùng để reset màn chơi hoặc sinh lại).
        /// </summary>
        private void ClearPreviousMap()
        {
            foreach (var room in spawnedRooms)
            {
                if (room != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        DestroyImmediate(room.gameObject);
                    }
                    else
#endif
                    {
                        Destroy(room.gameObject);
                    }
                }
            }
            spawnedRooms.Clear();
        }

        /// <summary>
        /// Thực hiện thử nghiệm sinh bản đồ một lần.
        /// </summary>
        private bool TryGenerateMap()
        {
            if (startRoomPrefab == null || bossRoomPrefab == null || rewardRoomPrefab == null || combatRoomPrefabs.Length == 0)
            {
                Debug.LogError("[MapGenerator] Thiếu cấu hình các room Prefab trong Inspector!");
                return false;
            }

            // 1. Sinh Start Room tại vị trí mặc định của prefab (để giữ nguyên trục Z cấu hình sẵn)
            Vector3 startPos = startRoomPrefab.transform.position;
            GameObject startRoomObj = Instantiate(startRoomPrefab, startPos, Quaternion.identity);
            startRoomObj.transform.SetParent(this.transform);
            RoomManager startRoom = startRoomObj.GetComponent<RoomManager>();
            startRoom.gameObject.name = "StartRoom_0";
            startRoom.roomType = RoomType.Start;
            spawnedRooms.Add(startRoom);

            int currentRoomCount = 1;
            int roomAttempts = 0;
            int maxRoomAttempts = 100; // Số lượt thử ghép nối cho một phòng cụ thể trước khi đổi phòng gốc khác

            // Hàng đợi các phòng có cửa còn trống để tiếp tục ghép nối (BFS-style)
            List<RoomManager> roomsWithOpenDoors = new List<RoomManager> { startRoom };

            while (currentRoomCount < totalRooms)
            {
                if (roomsWithOpenDoors.Count == 0)
                {
                    // Đã hết phòng có cửa trống nhưng chưa sinh đủ số lượng phòng mong muốn
                    return false;
                }

                // Chọn ngẫu nhiên một phòng đã có làm gốc (A)
                int randomRoomIndex = Random.Range(0, roomsWithOpenDoors.Count);
                RoomManager roomA = roomsWithOpenDoors[randomRoomIndex];

                // Lấy danh sách các cửa trống của phòng A
                List<RoomDoor> openDoorsA = GetOpenDoors(roomA);

                if (openDoorsA.Count == 0)
                {
                    // Phòng A không còn cửa trống nào nữa, loại khỏi danh sách chờ
                    roomsWithOpenDoors.RemoveAt(randomRoomIndex);
                    continue;
                }

                // Chọn ngẫu nhiên 1 cửa trống của phòng A
                RoomDoor doorA = openDoorsA[Random.Range(0, openDoorsA.Count)];

                // Xác định loại phòng tiếp theo
                RoomType nextRoomType = RoomType.Combat;
                if (currentRoomCount == totalRooms - 1)
                {
                    nextRoomType = RoomType.Boss;
                }
                else if (currentRoomCount == totalRooms - 2)
                {
                    nextRoomType = RoomType.Reward;
                }

                // Chọn ngẫu nhiên prefab tương ứng
                GameObject nextRoomPrefab = GetPrefabForType(nextRoomType);
                if (nextRoomPrefab == null) return false;

                // Instantiate tạm thời phòng mới (B) ở xa để lấy thông tin cấu hình cửa
                Vector3 tempSpawnPos = new Vector3(0, 9999f + (currentRoomCount * 50f), 0);
                GameObject roomBObj = Instantiate(nextRoomPrefab, tempSpawnPos, Quaternion.identity);
                RoomManager roomB = roomBObj.GetComponent<RoomManager>();
                roomB.roomType = nextRoomType;

                // Tìm cửa của phòng B có hướng đối lập với doorA để ghép nối
                DoorDirection oppositeDir = GetOppositeDirection(doorA.direction);
                List<RoomDoor> validDoorsB = GetDoorsOfDirection(roomB, oppositeDir);

                if (validDoorsB.Count == 0)
                {
                    // Không tìm thấy cửa khớp hướng, hủy phòng tạm thời và thử lại
                    Destroy(roomBObj);
                    roomAttempts++;
                    if (roomAttempts > maxRoomAttempts)
                    {
                        return false; // Thử lại toàn bộ map
                    }
                    continue;
                }

                // Chọn ngẫu nhiên một cửa B phù hợp
                RoomDoor doorB = validDoorsB[Random.Range(0, validDoorsB.Count)];

                // Tính toán vị trí thế giới để đặt phòng B sao cho cửa B trùng khít cửa A
                // posB = worldPosDoorA - localPosDoorB
                Vector3 targetPosB = doorA.transform.position - (Vector3)doorB.GetLocalOffset();
                targetPosB.z = nextRoomPrefab.transform.position.z; // Khóa trục Z theo cấu hình của Prefab phòng mới
                roomBObj.transform.position = targetPosB;

                // Kiểm tra va chạm (Overlap check) trong không gian
                bool isOverlap = CheckRoomOverlap(roomB);

                if (isOverlap)
                {
                    // Phòng bị chồng lấn, hủy phòng tạm thời và thử lại
                    Destroy(roomBObj);
                    roomAttempts++;
                    if (roomAttempts > maxRoomAttempts)
                    {
                        return false; // Thử lại toàn bộ map
                    }
                }
                else
                {
                    // Đặt phòng thành công!
                    roomBObj.transform.SetParent(this.transform);
                    roomBObj.name = $"{nextRoomType}Room_{currentRoomCount}";
                    
                    // Kết nối hai cửa lại với nhau
                    doorA.ConnectTo(doorB);
                    doorB.ConnectTo(doorA);

                    spawnedRooms.Add(roomB);
                    roomsWithOpenDoors.Add(roomB);

                    currentRoomCount++;
                    roomAttempts = 0; // Reset số lần thử cho phòng tiếp theo
                }
            }

            // 3. Sau khi sinh xong toàn bộ, bịt các cửa thừa của tất cả các phòng
            CloseAllRemainingDoors();
            return true;
        }

        /// <summary>
        /// Kiểm tra xem phòng mới có bị chồng lấn lên các phòng đã tồn tại hay không.
        /// </summary>
        private bool CheckRoomOverlap(RoomManager room)
        {
            // Lấy Bounding Box của phòng
            Bounds roomBounds = room.GetRoomBounds();

            // Lấy Collider2D của chính nó để tạm thời tắt đi trước khi quét
            Collider2D selfCollider = room.GetComponent<Collider2D>();
            bool originalState = true;
            if (selfCollider != null)
            {
                originalState = selfCollider.enabled;
                selfCollider.enabled = false;
            }

            // Quét Overlap bằng OverlapBox trong Physics2D
            // Nhân kích thước với 0.95f để tránh việc nhận diện nhầm các đường ranh giới cửa chạm nhau
            Vector2 size = (Vector2)roomBounds.size * 0.95f;
            Collider2D hit = Physics2D.OverlapBox((Vector2)roomBounds.center, size, 0f, roomLayer);

            // Khôi phục lại trạng thái collider của chính nó
            if (selfCollider != null)
            {
                selfCollider.enabled = originalState;
            }

            return hit != null;
        }

        /// <summary>
        /// Bịt kín tất cả các cửa không kết nối của mọi phòng đã sinh.
        /// </summary>
        private void CloseAllRemainingDoors()
        {
            foreach (var room in spawnedRooms)
            {
                RoomDoor[] doors = room.GetDoors();
                foreach (var door in doors)
                {
                    if (door != null && !door.IsConnected)
                    {
                        door.CloseDoorPermanently();
                    }
                }
            }
        }

        /// <summary>
        /// Lấy tất cả các cửa chưa kết nối của một phòng.
        /// </summary>
        private List<RoomDoor> GetOpenDoors(RoomManager room)
        {
            List<RoomDoor> openDoors = new List<RoomDoor>();
            RoomDoor[] allDoors = room.GetDoors();
            foreach (var door in allDoors)
            {
                if (door != null && !door.IsConnected)
                {
                    openDoors.Add(door);
                }
            }
            return openDoors;
        }

        /// <summary>
        /// Lấy danh sách các cửa của phòng theo một hướng cụ thể.
        /// </summary>
        private List<RoomDoor> GetDoorsOfDirection(RoomManager room, DoorDirection dir)
        {
            List<RoomDoor> doors = new List<RoomDoor>();
            RoomDoor[] allDoors = room.GetDoors();
            foreach (var door in allDoors)
            {
                if (door != null && door.direction == dir)
                {
                    doors.Add(door);
                }
            }
            return doors;
        }

        /// <summary>
        /// Lấy hướng đối lập của một hướng cho trước.
        /// </summary>
        private DoorDirection GetOppositeDirection(DoorDirection dir)
        {
            switch (dir)
            {
                case DoorDirection.Left: return DoorDirection.Right;
                case DoorDirection.Right: return DoorDirection.Left;
                case DoorDirection.Up: return DoorDirection.Down;
                case DoorDirection.Down: return DoorDirection.Up;
                default: return DoorDirection.Right;
            }
        }

        /// <summary>
        /// Lấy prefab ngẫu nhiên cho một loại phòng cụ thể.
        /// </summary>
        private GameObject GetPrefabForType(RoomType type)
        {
            switch (type)
            {
                case RoomType.Start:
                    return startRoomPrefab;
                case RoomType.Combat:
                    if (combatRoomPrefabs == null || combatRoomPrefabs.Length == 0) return null;
                    return combatRoomPrefabs[Random.Range(0, combatRoomPrefabs.Length)];
                case RoomType.Reward:
                    return rewardRoomPrefab;
                case RoomType.Boss:
                    return bossRoomPrefab;
                default:
                    return null;
            }
        }
    }
}
