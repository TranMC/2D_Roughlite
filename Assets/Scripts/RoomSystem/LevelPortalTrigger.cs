using UnityEngine;
using Roguelite.Core;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Component Trigger đặt ở khu vực Portal/Cửa ra (dùng chung cho mọi phòng/màn chơi/shop).
    /// Tự động nhận diện Player và gọi SceneTransitionManager chuyển sang Scene kế tiếp theo danh sách Sequence.
    /// Version: 1.2.0
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelPortalTrigger : MonoBehaviour
    {
        public const string VERSION = "1.2.0";

        [Header("Scene Transition Settings")]
        [Tooltip("Tự động Auto-Save khi vừa vào scene mới")]
        [SerializeField] private bool autoSaveOnArrival = true;

        [Tooltip("Layer nhận diện Player (Tùy chọn nếu không gán Tag 'Player')")]
        [SerializeField] private LayerMask playerLayer;

        [Header("Portal Requirements (Tùy chọn)")]
        [Tooltip("Nếu tích chọn, Portal chỉ kích hoạt khi phòng chiến đấu đã dọn sạch quái. Trong Shop không có quái sẽ tự động qua.")]
        [SerializeField] private bool requireRoomCleared = false; // Mặc định false để dùng mượt ở mọi scene kể cả Shop

        [SerializeField] private GameObject activeVisuals;

        private bool isTriggered = false;
        private RoomManager parentRoom;

        private void Awake()
        {
            parentRoom = GetComponentInParent<RoomManager>();
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Start()
        {
            if (activeVisuals != null)
            {
                activeVisuals.SetActive(IsRoomClearedOrNotRequired());
            }
        }

        private void Update()
        {
            if (requireRoomCleared && parentRoom != null && !isTriggered)
            {
                if (IsRoomClearedOrNotRequired())
                {
                    if (activeVisuals != null && !activeVisuals.activeSelf)
                    {
                        activeVisuals.SetActive(true);
                    }
                }
            }
        }

        private bool IsRoomClearedOrNotRequired()
        {
            if (!requireRoomCleared || parentRoom == null) return true;

            // Nếu phòng có EnemySpawner thì mới bắt buộc kiểm tra dọn quái.
            // Nếu là Shop hoặc phòng không có Spawner -> Tự động tính là đã Clear.
            EnemySpawner spawner = parentRoom.GetComponent<EnemySpawner>();
            if (spawner == null) return true;

            return parentRoom.IsCleared;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isTriggered) return;

            // Kiểm tra điều kiện dọn quái
            if (!IsRoomClearedOrNotRequired())
            {
                Debug.Log("[LevelPortalTrigger] Phòng chưa dọn sạch quái, portal tạm khóa!");
                return;
            }

            // Nhận diện Player đa năng (Tag "Player" OR Component PlayerController OR LayerMask)
            bool isPlayer = collision.CompareTag("Player") 
                         || collision.GetComponentInParent<PlayerController>() != null 
                         || collision.GetComponent<PlayerController>() != null 
                         || (playerLayer != 0 && ((1 << collision.gameObject.layer) & playerLayer) != 0);

            if (isPlayer)
            {
                isTriggered = true;
                Debug.Log("[LevelPortalTrigger] Player bước vào portal! Tự động chuyển sang Scene kế tiếp...");

                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToNextInSequence(autoSaveOnArrival);
                }
                else
                {
                    Debug.LogWarning("[LevelPortalTrigger] SceneTransitionManager Instance chưa có trong Scene, đang tự động tạo...");
                    GameObject transitionGo = new GameObject("SceneTransitionManager");
                    var manager = transitionGo.AddComponent<SceneTransitionManager>();
                    manager.TransitionToNextInSequence(autoSaveOnArrival);
                }
            }
        }

        /// <summary>
        /// Kích hoạt portal bằng code
        /// </summary>
        public void ForceActivatePortal()
        {
            if (activeVisuals != null)
            {
                activeVisuals.SetActive(true);
            }
        }
    }
}
