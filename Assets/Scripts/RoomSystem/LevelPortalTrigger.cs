using UnityEngine;
using Roguelite.Core;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Component Trigger đặt ở khu vực Portal/Cửa ra (dùng chung cho mọi phòng/màn chơi/shop).
    /// Tự động nhận diện Player và gọi SceneTransitionManager chuyển sang Scene kế tiếp theo danh sách Sequence hoặc Scene chỉ định.
    /// Version: 1.3.0
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelPortalTrigger : MonoBehaviour
    {
        public const string VERSION = "1.3.0";

        [Header("Scene Transition Settings")]
        [Tooltip("Chuyển trực tiếp tới Scene này (nếu để trống sẽ tự động tịnh tiến theo Sequence trong SceneTransitionManager).")]
        [SerializeField] private string targetSceneName = "";

        [Tooltip("Tự động Auto-Save khi vừa vào scene mới")]
        [SerializeField] private bool autoSaveOnArrival = true;

        [Tooltip("Layer nhận diện Player (Tùy chọn nếu không gán Tag 'Player')")]
        [SerializeField] private LayerMask playerLayer;

        [Header("Portal Requirements (Tùy chọn)")]
        [Tooltip("Nếu tích chọn, Portal chỉ kích hoạt khi phòng chiến đấu đã dọn sạch quái. Trong Shop không có quái sẽ tự động qua.")]
        [SerializeField] private bool requireRoomCleared = false; // Mặc định false để dùng mượt ở mọi scene kể cả Shop

        [SerializeField] private GameObject activeVisuals;

        [Header("Audio & FX (Tùy chọn)")]
        [SerializeField] private AudioClip portalEnterSFX;

        private bool isTriggered = false;
        private RoomManager parentRoom;
        private Collider2D triggerCollider;

        private void Awake()
        {
            parentRoom = GetComponentInParent<RoomManager>();
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void Start()
        {
            if (parentRoom == null)
            {
                parentRoom = GetComponentInParent<RoomManager>();
            }

            UpdateVisuals();
        }

        private void Update()
        {
            if (!isTriggered)
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (activeVisuals != null)
            {
                bool shouldBeActive = IsRoomClearedOrNotRequired();
                if (activeVisuals.activeSelf != shouldBeActive)
                {
                    activeVisuals.SetActive(shouldBeActive);
                }
            }
        }

        private bool IsRoomClearedOrNotRequired()
        {
            if (!requireRoomCleared) return true;

            if (parentRoom == null)
            {
                parentRoom = GetComponentInParent<RoomManager>();
                if (parentRoom == null)
                {
                    // Fallback: Tìm RoomManager trong Scene nếu có
                    parentRoom = FindObjectOfType<RoomManager>();
                }
            }

            if (parentRoom == null) return true;

            // Nếu phòng có EnemySpawner thì mới bắt buộc kiểm tra dọn quái.
            // Nếu là Shop hoặc phòng không có Spawner -> Tự động tính là đã Clear.
            EnemySpawner spawner = parentRoom.GetComponent<EnemySpawner>();
            if (spawner == null) return true;

            return parentRoom.IsCleared;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryTriggerPortal(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryTriggerPortal(collision);
        }

        private void TryTriggerPortal(Collider2D collision)
        {
            if (isTriggered) return;

            // Chặn kích hoạt nếu SceneTransitionManager đang chuyển scene
            if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
            {
                return;
            }

            // Nhận diện Player đa phương thức (Tag, Component PlayerController, PlayerStats, LayerMask, hoặc Name)
            if (!IsPlayer(collision)) return;

            // Kiểm tra điều kiện dọn quái
            if (!IsRoomClearedOrNotRequired())
            {
                return;
            }

            isTriggered = true;
            Debug.Log($"[LevelPortalTrigger] ✅ Player bước vào portal '{gameObject.name}'! Tiến hành chuyển Scene...");

            // Phát âm thanh portal nếu có
            if (portalEnterSFX != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(portalEnterSFX, transform.position);
            }

            // Đảm bảo SceneTransitionManager luôn tồn tại
            EnsureSceneTransitionManager();

            if (SceneTransitionManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.Log($"[LevelPortalTrigger] Chuyển tới Scene chỉ định: '{targetSceneName}'");
                    SceneTransitionManager.Instance.TransitionToScene(targetSceneName, autoSaveOnArrival);
                }
                else
                {
                    Debug.Log("[LevelPortalTrigger] Tịnh tiến tới Scene kế tiếp trong Sequence...");
                    SceneTransitionManager.Instance.TransitionToNextInSequence(autoSaveOnArrival);
                }
            }
            else
            {
                Debug.LogError("[LevelPortalTrigger] ❌ Không thể khởi tạo SceneTransitionManager!");
            }
        }

        private bool IsPlayer(Collider2D collision)
        {
            if (collision == null) return false;

            // 1. Kiểm tra Tag
            if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
            {
                return true;
            }

            // 2. Kiểm tra Component PlayerController hoặc PlayerStats
            if (collision.GetComponentInParent<PlayerController>() != null ||
                collision.GetComponent<PlayerController>() != null ||
                collision.GetComponentInParent<Roguelite.Player.PlayerStats>() != null ||
                collision.GetComponent<Roguelite.Player.PlayerStats>() != null)
            {
                return true;
            }

            // 3. Kiểm tra Layer
            if (playerLayer.value != 0 && ((1 << collision.gameObject.layer) & playerLayer.value) != 0)
            {
                return true;
            }

            // 4. Fallback theo tên GameObject
            if (collision.gameObject.name.Equals("Player", System.StringComparison.OrdinalIgnoreCase) ||
                collision.transform.root.gameObject.name.Equals("Player", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void EnsureSceneTransitionManager()
        {
            if (SceneTransitionManager.Instance == null)
            {
                SceneTransitionManager existing = FindObjectOfType<SceneTransitionManager>();
                if (existing == null)
                {
                    Debug.LogWarning("[LevelPortalTrigger] SceneTransitionManager Instance chưa có, tự động tạo mới...");
                    GameObject transitionGo = new GameObject("SceneTransitionManager");
                    transitionGo.AddComponent<SceneTransitionManager>();
                }
            }
        }

        /// <summary>
        /// Kích hoạt portal bằng code hoặc reset trạng thái trigger
        /// </summary>
        public void ForceActivatePortal()
        {
            if (activeVisuals != null)
            {
                activeVisuals.SetActive(true);
            }
        }

        public void ResetPortalTrigger()
        {
            isTriggered = false;
        }
    }
}
