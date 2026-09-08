using UnityEngine;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Quản lý trạng thái, kết nối vật lý và vật cản của từng cửa trong phòng.
    /// Version: 1.2.1
    /// </summary>
    public class RoomDoor : MonoBehaviour
    {
        public const string VERSION = "1.2.1";

        [Header("Cấu hình Cửa")]
        [Tooltip("Hướng của cửa này.")]
        public DoorDirection direction;

        [Tooltip("Đối tượng tường dùng để bịt cửa khi không kết nối với phòng nào.")]
        [SerializeField] private GameObject wallObject;

        [Tooltip("Cửa sắt/chặn dùng để nhốt người chơi khi phòng đang chiến đấu (LockRoom).")]
        [SerializeField] private GameObject gateObject;

        [Header("Trạng thái Kết nối (Runtime)")]
        [SerializeField] private bool isConnected = false;
        public bool IsConnected => isConnected;

        [SerializeField] private RoomDoor connectedDoor;
        public RoomDoor ConnectedDoor => connectedDoor;

        private bool isClosedPermanently = false;

        private RoomManager ownerRoom;
        public RoomManager OwnerRoom => ownerRoom;

        private void Awake()
        {
            // Tìm RoomManager cha của cửa này
            ownerRoom = GetComponentInParent<RoomManager>();
            if (ownerRoom == null)
            {
                Debug.LogWarning($"[RoomDoor] [{gameObject.name}] Không tìm thấy RoomManager ở các GameObject cha!");
            }

            // Nếu chưa gán gateObject thì tự động gán chính GameObject này nếu có Collider
            if (gateObject == null && GetComponent<Collider2D>() != null)
            {
                gateObject = gameObject;
            }
        }

        /// <summary>
        /// Lấy vị trí tương đối (offset) của cửa so với tâm phòng.
        /// </summary>
        public Vector2 GetLocalOffset()
        {
            return (Vector2)transform.localPosition;
        }

        /// <summary>
        /// Thực hiện ghép nối cửa này với cửa của phòng bên kia.
        /// </summary>
        public void ConnectTo(RoomDoor otherDoor)
        {
            isConnected = true;
            connectedDoor = otherDoor;

            // Ẩn tường chắn đi vì hướng này đã được thông cửa
            if (wallObject != null)
            {
                wallObject.SetActive(false);
            }

            // Đảm bảo ban đầu cửa chặn combat không hoạt động
            if (gateObject != null)
            {
                gateObject.SetActive(false);
            }
        }

        public void CloseDoorPermanently()
        {
            if (ownerRoom == null)
            {
                ownerRoom = GetComponentInParent<RoomManager>();
            }

            // Tuyệt đối không đóng vĩnh viễn cửa của phòng Boss,
            // vì cửa phải (RightDoor) là lối thoát tới Portal sau khi hạ gục Boss!
            if (ownerRoom != null && ownerRoom.roomType == RoomType.Boss)
            {
                return;
            }

            isConnected = false;
            connectedDoor = null;
            isClosedPermanently = true;

            // Bật tường chắn lên để chặn lối đi hoàn toàn
            if (wallObject != null)
            {
                wallObject.SetActive(true);
                if (gateObject != null && gateObject != wallObject)
                {
                    gateObject.SetActive(false);
                }
            }
            else if (gateObject != null)
            {
                // Nếu phòng không có wallObject riêng (như CombatRoom),
                // chính gateObject (với Collider2D) phải luôn được BẬT để đóng kín cửa,
                // ngăn không cho quái hay người chơi lọt ra ngoài phòng!
                gateObject.SetActive(true);
            }
        }

        /// <summary>
        /// Kích hoạt hoặc vô hiệu hóa cửa chặn lối đi (dùng khi phòng bị khóa/mở).
        /// </summary>
        public void SetGateActive(bool isActive)
        {
            if (gateObject == null) return;

            if (ownerRoom == null)
            {
                ownerRoom = GetComponentInParent<RoomManager>();
            }

            // Với phòng Boss: Cửa luôn tuân theo lệnh đóng/mở của RoomManager
            // - Khi phòng bị khóa (LockRoom): isActive = true -> Khóa cửa (cả cửa vào và cửa ra Portal) để nhốt Boss
            // - Khi phòng hoàn thành (OpenDoors): isActive = false -> Mở cửa để người chơi đi tới Portal!
            if (ownerRoom != null && ownerRoom.roomType == RoomType.Boss)
            {
                gateObject.SetActive(isActive);
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = isActive;
                }
                return;
            }

            // Nếu cửa này đã bị đóng vĩnh viễn (không có phòng nối ở các phòng thường)
            if (isClosedPermanently)
            {
                // Nếu không có wallObject thì gateObject bắt buộc phải luôn BẬT để đóng vai trò làm tường chắn
                if (wallObject == null)
                {
                    gateObject.SetActive(true);
                }
                return;
            }

            // Với cửa có kết nối hợp lệ: bật khi phòng bị khóa chiến đấu, tắt khi dọn phòng xong
            gateObject.SetActive(isActive);
        }

        // Dùng để vẽ trực quan hướng cửa trong Editor
        private void OnDrawGizmos()
        {
            Gizmos.color = isConnected ? Color.green : Color.red;
            Gizmos.DrawSphere(transform.position, 0.25f);
            
            // Vẽ đường nối nếu đã kết nối
            if (isConnected && connectedDoor != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, connectedDoor.transform.position);
            }
        }
    }
}
