using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Roguelite.SaveSystem;

namespace Roguelite.Core
{
    /// <summary>
    /// Quản lý chuyển Scene mượt mà (Fade Screen, Load Async, Auto Save khi vào Scene trung chuyển/Shop)
    /// Version: 1.3.0
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public const string VERSION = "1.3.0";
        public static SceneTransitionManager Instance { get; private set; }

        [Header("UI Canvas Overlay")]
        [SerializeField] private CanvasGroup faderCanvasGroup;
        [SerializeField] private Image faderImage;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Transition Settings")]
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float minimumLoadingTime = 0.4f;

        [Header("Sequence Settings")]
        [Tooltip("Thứ tự danh sách các Scene trong lượt chơi. Ví dụ: Scene1 -> ShopScene -> Scene2 -> ShopScene -> BossScene")]
        [SerializeField] private string[] sceneSequence = new string[] { "Scene1", "ShopScene", "Scene2", "ShopScene", "BossScene" };
        [SerializeField] private int currentSequenceIndex = 0;

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureFaderSetup();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void EnsureFaderSetup()
        {
            if (faderCanvasGroup == null)
            {
                faderCanvasGroup = GetComponentInChildren<CanvasGroup>();
                if (faderCanvasGroup == null)
                {
                    GameObject canvasObj = new GameObject("TransitionCanvas");
                    canvasObj.transform.SetParent(transform);

                    Canvas canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 999; // Trên cùng màn hình

                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();

                    faderCanvasGroup = canvasObj.AddComponent<CanvasGroup>();

                    GameObject imgObj = new GameObject("FaderImage");
                    imgObj.transform.SetParent(canvasObj.transform, false);
                    faderImage = imgObj.AddComponent<Image>();
                    faderImage.color = Color.black;

                    RectTransform rect = imgObj.GetComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.one;
                }
            }

            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
                faderCanvasGroup.blocksRaycasts = false;
            }

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Chuyển tới Scene mới mượt mà với hiệu ứng Fade mờ dần và Auto-Save (nếu cần)
        /// </summary>
        public void TransitionToScene(string targetSceneName, bool autoSaveOnArrival = true, Action onComplete = null)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Đang trong quá trình chuyển scene!");
                return;
            }

            // Kiểm tra xem Scene có tồn tại trong Build Settings không
            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"[SceneTransitionManager] ❌ LỖI CHUYỂN SCENE: Scene '{targetSceneName}' KHÔNG TỒN TẠI hoặc chưa được thêm vào 'File -> Build Settings'! (Index trong Sequence: {currentSequenceIndex})");
#if UNITY_EDITOR
                Debug.Break(); // Tạm dừng Game ngay lập tức trong Unity Editor để báo lỗi
#endif
                return;
            }

            StartCoroutine(DoSceneTransition(targetSceneName, autoSaveOnArrival, onComplete));
        }

        private IEnumerator DoSceneTransition(string targetSceneName, bool autoSaveOnArrival, Action onComplete)
        {
            IsTransitioning = true;
            EnsureFaderSetup();

            // Block tương tác chuột/bàn phím trong lúc chuyển màn
            faderCanvasGroup.blocksRaycasts = true;

            // 1. Fade Out màn hình (Màn hình tối dần thành đen)
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                faderCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
            faderCanvasGroup.alpha = 1f;

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(true);
            }

            // 2. Load Scene bất đồng bộ (Async)
            float startTime = Time.unscaledTime;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // Chờ tối thiểu để chuyển cảnh mượt
            while (Time.unscaledTime - startTime < minimumLoadingTime)
            {
                yield return null;
            }

            // Cho phép kích hoạt scene mới
            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
            }

            // 3. Nếu là Scene trung chuyển / Shop -> Kích hoạt Auto Save
            if (autoSaveOnArrival && SaveManager.Instance != null)
            {
                Debug.Log($"[SceneTransitionManager] Đã tới scene '{targetSceneName}', kích hoạt Auto-Save...");
                SaveManager.Instance.TriggerAutoSave(0.2f);
            }

            // 4. Fade In màn hình (Sáng dần màn hình scene mới)
            elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                faderCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeInDuration));
                yield return null;
            }

            faderCanvasGroup.alpha = 0f;
            faderCanvasGroup.blocksRaycasts = false;
                
            IsTransitioning = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Tự động chuyển sang Scene kế tiếp theo danh sách tịnh tiến tuyến tính trong sceneSequence
        /// </summary>
        public void TransitionToNextInSequence(bool autoSaveOnArrival = true, Action onComplete = null)
        {
            if (sceneSequence == null || sceneSequence.Length == 0)
            {
                Debug.LogError("[SceneTransitionManager] ❌ LỖI: Chưa cấu hình danh sách sceneSequence trong Inspector!");
#if UNITY_EDITOR
                Debug.Break();
#endif
                return;
            }

            string activeSceneName = SceneManager.GetActiveScene().name;

            // Nếu currentSequenceIndex khớp với scene hiện tại, tăng index tuyến tính
            if (currentSequenceIndex < sceneSequence.Length && 
                string.Equals(sceneSequence[currentSequenceIndex], activeSceneName, StringComparison.OrdinalIgnoreCase))
            {
                currentSequenceIndex = (currentSequenceIndex + 1) % sceneSequence.Length;
            }
            else
            {
                // Nếu chưa khớp (ví dụ mở Scene trực tiếp trong Editor), tìm index hiện tại từ vị trí currentSequenceIndex trở đi
                int foundIndex = -1;
                for (int i = 0; i < sceneSequence.Length; i++)
                {
                    int checkIdx = (currentSequenceIndex + i) % sceneSequence.Length;
                    if (string.Equals(sceneSequence[checkIdx], activeSceneName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundIndex = checkIdx;
                        break;
                    }
                }

                if (foundIndex >= 0)
                {
                    currentSequenceIndex = (foundIndex + 1) % sceneSequence.Length;
                }
                else
                {
                    currentSequenceIndex = (currentSequenceIndex + 1) % sceneSequence.Length;
                }
            }

            string nextScene = sceneSequence[currentSequenceIndex];
            Debug.Log($"[SceneTransitionManager] 🔄 Tịnh tiến từ '[{activeSceneName}]' sang Scene tiếp theo ở Sequence Index {currentSequenceIndex}: '{nextScene}'");

            TransitionToScene(nextScene, autoSaveOnArrival, onComplete);
        }

        /// <summary>
        /// Đặt lại Sequence về vị trí ban đầu (Scene1) khi bắt đầu lại lượt chơi (Restart Run)
        /// </summary>
        public void ResetSequence()
        {
            currentSequenceIndex = 0;
        }
    }
}
