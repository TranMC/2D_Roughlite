using UnityEngine;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Quản lý trạng thái, kết nối vật lý và vật cản của từng cửa trong phòng.
    /// </summary>
    public class RoomDoor : MonoBehaviour
    {
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
            isConnected = false;
            connectedDoor = null;
            isClosedPermanently = true;

            // Bật tường chắn lên để chặn lối đi hoàn toàn
            if (wallObject != null)
            {
                wallObject.SetActive(true);
            }

            // Tắt cửa chặn combat
            if (gateObject != null)
            {
                gateObject.SetActive(false);
            }
        }

        /// <summary>
        /// Kích hoạt hoặc vô hiệu hóa cửa chặn lối đi (dùng khi phòng bị khóa/mở).
        /// </summary>
        public void SetGateActive(bool isActive)
        {
            // Chỉ hoạt động nếu cửa không bị bịt kín vĩnh viễn
            if (gateObject != null && !isClosedPermanently)
            {
                gateObject.SetActive(isActive);
            }
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
