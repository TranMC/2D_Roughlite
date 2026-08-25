using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Roguelite.SaveSystem;

namespace Roguelite.Core
{
    /// <summary>
    /// Quản lý chuyển Scene mượt mà (Fade Screen, Load Async, Progress Bar, Tips ngẫu nhiên, Auto Save, Auto EventSystem).
    /// Hỗ trợ đầy đủ cả khi TransitionCanvas bị Disable (SetActive = false) trong Hierarchy.
    /// Version: 1.5.3
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public const string VERSION = "1.5.3";
        public static SceneTransitionManager Instance { get; private set; }

        [Header("UI Canvas Overlay")]
        [SerializeField] private CanvasGroup faderCanvasGroup;
        [SerializeField] private Image faderImage;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private Slider progressBar;

        [Tooltip("Có thể kéo thả Sprite (Project), Image UI hoặc GameObject (Hierarchy) vào đây đều nhận được!")]
        [SerializeField] private UnityEngine.Object spinnerIcon;

        [Header("Transition Settings")]
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float minimumLoadingTime = 0.6f;
        [SerializeField] private float spinnerRotateSpeed = 200f;

        [Header("Game Tips List")]
        [SerializeField] private string[] gameTips = new string[]
        {
            "Mẹo: Có một số môi trường ngoại vi có thể gây sát thương, hãy cẩn thận.",
            "Mẹo: Đừng quên ghé thăm Shop để hồi phục thể lực và sắm vũ khí mới.",
            "Mẹo: Mỗi loại kẻ địch có sơ hở riêng, hãy quan sát chuyển động của chúng.",
            "Mẹo: Thu thập Coin để nâng cấp chỉ số vĩnh viễn cho nhân vật.",
            "Mẹo: Quản lý Perk phù hợp cho mỗi lượt chơi!",
            "Mẹo: Hãy chú ý nhịp đánh của Boss để né tránh kịp thời."
        };

        [Header("Sequence Settings")]
        [Tooltip("Thứ tự danh sách các Scene trong lượt chơi. Ví dụ: Scene1 -> ShopScene -> Scene2 -> ShopScene -> BossScene")]
        [SerializeField] private string[] sceneSequence = new string[] { "Scene1", "ShopScene", "Scene2", "ShopScene", "BossScene" };
        [SerializeField] private int currentSequenceIndex = 0;

        public bool IsTransitioning { get; private set; }
        public int CurrentSequenceIndex => currentSequenceIndex;
        public string[] SceneSequence => sceneSequence;

        private Image activeSpinnerImage;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureFaderSetup();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Đảm bảo trạng thái fader luôn ẩn và tắt raycast lúc vào game
            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
                faderCanvasGroup.blocksRaycasts = false;
                faderCanvasGroup.interactable = false;
                faderCanvasGroup.gameObject.SetActive(false);
            }
            SetLoadingUIActive(false);
        }

        public void EnsureFaderSetup()
        {
            // 1. Tìm faderCanvasGroup trong con trực tiếp (kể cả khi đang bị Disable / Inactive)
            if (faderCanvasGroup == null)
            {
                faderCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
            }

            // 2. Nếu chưa có, quét toàn bộ Scene tìm TransitionCanvas thủ công (kể cả Inactive)
            if (faderCanvasGroup == null)
            {
                Canvas customCanvas = FindCustomTransitionCanvas();
                if (customCanvas != null)
                {
                    // Chuyển canvas này làm con trực tiếp của SceneTransitionManager để bảo toàn qua DontDestroyOnLoad
                    customCanvas.transform.SetParent(transform, false);

                    faderCanvasGroup = customCanvas.GetComponent<CanvasGroup>();
                    if (faderCanvasGroup == null)
                    {
                        faderCanvasGroup = customCanvas.gameObject.AddComponent<CanvasGroup>();
                    }

                    Debug.Log($"[SceneTransitionManager] ✅ Đã tự động liên kết '{customCanvas.name}' làm con của SceneTransitionManager.");
                }
            }
            else
            {
                // Nếu faderCanvasGroup đang nằm ngoài SceneTransitionManager, reparent vào
                if (faderCanvasGroup.transform.parent != transform)
                {
                    faderCanvasGroup.transform.SetParent(transform, false);
                }
            }

            // 3. Nếu vẫn không tìm thấy bất kỳ Canvas nào trong Scene, tạo mới bằng Code
            if (faderCanvasGroup == null)
            {
                CreateDefaultTransitionCanvas();
                Debug.Log("[SceneTransitionManager] ℹ️ Đã tự động tạo TransitionCanvas mặc định bằng Code.");
            }
            else
            {
                // Tự động tìm kiếm và liên kết các thành phần con (FaderImage, LoadingText, TipText, ProgressBar, SpinnerIcon)
                AutoBindChildElements(faderCanvasGroup.gameObject);
            }

            // 4. Chuẩn hóa cấu hình Canvas (ScreenSpaceOverlay, sortingOrder 9999)
            if (faderCanvasGroup != null)
            {
                Canvas rootCanvas = faderCanvasGroup.GetComponent<Canvas>();
                if (rootCanvas != null)
                {
                    rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    rootCanvas.overrideSorting = true;
                    rootCanvas.sortingOrder = 9999;
                }

                CanvasScaler scaler = faderCanvasGroup.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = faderCanvasGroup.gameObject.AddComponent<CanvasScaler>();
                }
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                if (faderCanvasGroup.GetComponent<GraphicRaycaster>() == null)
                {
                    faderCanvasGroup.gameObject.AddComponent<GraphicRaycaster>();
                }

                faderCanvasGroup.alpha = 0f;
                faderCanvasGroup.blocksRaycasts = false;
                faderCanvasGroup.interactable = false;
            }

            ResolveSpinnerReference();
            SetLoadingUIActive(false);
        }

        private Canvas FindCustomTransitionCanvas()
        {
            Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
            foreach (var c in allCanvases)
            {
                if (c.name.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.name.IndexOf("Fader", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.name.IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return c;
                }
            }
            return null;
        }

        private void CreateDefaultTransitionCanvas()
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            faderCanvasGroup = canvasObj.AddComponent<CanvasGroup>();

            // Background Fader Image
            GameObject imgObj = new GameObject("FaderImage");
            imgObj.transform.SetParent(canvasObj.transform, false);
            faderImage = imgObj.AddComponent<Image>();
            faderImage.color = new Color(0.05f, 0.05f, 0.08f, 1f);

            RectTransform rect = imgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Container cho Loading Elements
            GameObject loadingPanelObj = new GameObject("LoadingPanel");
            loadingPanelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform loadingRect = loadingPanelObj.AddComponent<RectTransform>();
            loadingRect.anchorMin = Vector2.zero;
            loadingRect.anchorMax = Vector2.one;
            loadingRect.offsetMin = Vector2.zero;
            loadingRect.offsetMax = Vector2.zero;

            // Text Game Tip (Ở giữa / Phía dưới)
            GameObject tipObj = new GameObject("TipText");
            tipObj.transform.SetParent(loadingPanelObj.transform, false);
            tipText = tipObj.AddComponent<TextMeshProUGUI>();
            tipText.alignment = TextAlignmentOptions.Center;
            tipText.fontSize = 24;
            tipText.color = new Color(0.9f, 0.9f, 0.95f, 1f);
            RectTransform tipRect = tipObj.GetComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.1f, 0.16f);
            tipRect.anchorMax = new Vector2(0.9f, 0.26f);
            tipRect.offsetMin = Vector2.zero;
            tipRect.offsetMax = Vector2.zero;

            // Text Loading % (Ở góc phải dưới)
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(loadingPanelObj.transform, false);
            loadingText = textObj.AddComponent<TextMeshProUGUI>();
            loadingText.alignment = TextAlignmentOptions.Right;
            loadingText.fontSize = 24;
            loadingText.fontStyle = FontStyles.Bold;
            loadingText.color = Color.white;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.6f, 0.06f);
            textRect.anchorMax = new Vector2(0.9f, 0.12f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Progress Bar (Slider)
            GameObject sliderObj = new GameObject("ProgressBar");
            sliderObj.transform.SetParent(loadingPanelObj.transform, false);
            progressBar = sliderObj.AddComponent<Slider>();
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.1f, 0.04f);
            sliderRect.anchorMax = new Vector2(0.9f, 0.06f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // Fill Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Fill Area & Fill Image
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.75f, 1f, 1f);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            progressBar.fillRect = fillRect;
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;

            // Dynamic Spinner Icon Setup
            GameObject spinnerObj = new GameObject("SpinnerIcon");
            spinnerObj.transform.SetParent(loadingPanelObj.transform, false);
            activeSpinnerImage = spinnerObj.AddComponent<Image>();

            RectTransform spinnerRect = spinnerObj.GetComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.07f, 0.035f);
            spinnerRect.anchorMax = new Vector2(0.095f, 0.065f);
            spinnerRect.offsetMin = Vector2.zero;
            spinnerRect.offsetMax = Vector2.zero;
        }

        private void AutoBindChildElements(GameObject canvasRoot)
        {
            if (faderImage == null)
            {
                Transform t = canvasRoot.transform.Find("FaderImage");
                if (t != null) faderImage = t.GetComponent<Image>();
                if (faderImage == null)
                {
                    Image[] images = canvasRoot.GetComponentsInChildren<Image>(true);
                    if (images.Length > 0) faderImage = images[0];
                }
            }

            if (progressBar == null)
            {
                progressBar = canvasRoot.GetComponentInChildren<Slider>(true);
            }

            if (loadingText == null || tipText == null)
            {
                TextMeshProUGUI[] texts = canvasRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (loadingText == null && (t.name.IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Percent", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        loadingText = t;
                    }
                    else if (tipText == null && (t.name.IndexOf("Tip", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Hint", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        tipText = t;
                    }
                }

                if (texts.Length > 0 && tipText == null) tipText = texts[0];
                if (texts.Length > 1 && loadingText == null) loadingText = texts[1];
            }

            if (spinnerIcon == null)
            {
                Transform spinnerT = canvasRoot.transform.Find("LoadingPanel/SpinnerIcon");
                if (spinnerT == null) spinnerT = canvasRoot.transform.Find("SpinnerIcon");
                if (spinnerT != null)
                {
                    spinnerIcon = spinnerT.gameObject;
                }
                else
                {
                    Image[] images = canvasRoot.GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                    {
                        if (img.name.IndexOf("Spinner", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            spinnerIcon = img.gameObject;
                            break;
                        }
                    }
                }
            }
        }

        private void ResolveSpinnerReference()
        {
            if (spinnerIcon == null) return;

            if (spinnerIcon is Image img)
            {
                activeSpinnerImage = img;
            }
            else if (spinnerIcon is GameObject go && go.TryGetComponent<Image>(out var goImg))
            {
                activeSpinnerImage = goImg;
            }
            else if (spinnerIcon is Sprite spr)
            {
                if (activeSpinnerImage != null)
                {
                    activeSpinnerImage.sprite = spr;
                    activeSpinnerImage.color = Color.white;
                }
            }
        }

        private void SetLoadingUIActive(bool active)
        {
            if (faderCanvasGroup != null)
            {
                Transform loadingPanel = faderCanvasGroup.transform.Find("LoadingPanel");
                if (loadingPanel != null)
                {
                    loadingPanel.gameObject.SetActive(active);
                }
            }

            if (loadingText != null) loadingText.gameObject.SetActive(active);
            if (tipText != null) tipText.gameObject.SetActive(active);
            if (progressBar != null) progressBar.gameObject.SetActive(active);

            ResolveSpinnerReference();
            if (activeSpinnerImage != null)
            {
                activeSpinnerImage.gameObject.SetActive(active);
            }
        }

        private void DisplayRandomTip()
        {
            if (tipText != null && gameTips != null && gameTips.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, gameTips.Length);
                tipText.text = gameTips[randomIndex];
            }
        }

        /// <summary>
        /// Chuyển tới Scene mới mượt mà với hiệu ứng Fade mờ dần, thanh Loading, Game Tip ngẫu nhiên và Auto-Save.
        /// </summary>
        public void TransitionToScene(string targetSceneName, bool autoSaveOnArrival = true, Action onComplete = null)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] ⚠️ Đang trong quá trình chuyển scene, bỏ qua yêu cầu trùng lặp!");
                return;
            }

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("[SceneTransitionManager] ❌ Lỗi: targetSceneName rỗng!");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"[SceneTransitionManager] ❌ LỖI CHUYỂN SCENE: Scene '{targetSceneName}' KHÔNG TỒN TẠI hoặc chưa được thêm vào 'File -> Build Settings'! (Index Sequence: {currentSequenceIndex})");
                return;
            }

            StartCoroutine(DoSceneTransition(targetSceneName, autoSaveOnArrival, onComplete));
        }

        private IEnumerator DoSceneTransition(string targetSceneName, bool autoSaveOnArrival, Action onComplete)
        {
            IsTransitioning = true;
            EnsureFaderSetup();

            // KÍCH HOẠT GameObject của Canvas nếu người dùng disable ở Hierarchy
            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.gameObject.SetActive(true);
                faderCanvasGroup.blocksRaycasts = true;
                faderCanvasGroup.interactable = true;
                faderCanvasGroup.alpha = 0f;
            }

            // 1. Fade Out màn hình (Tối dần - dùng unscaledDeltaTime)
            float elapsed = 0f;
            float actualFadeOutDuration = Mathf.Max(fadeOutDuration, 0.1f);
            while (elapsed < actualFadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (faderCanvasGroup != null)
                {
                    faderCanvasGroup.alpha = Mathf.Clamp01(elapsed / actualFadeOutDuration);
                }
                yield return null;
            }

            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 1f;
            }

            // Kích hoạt giao diện Loading + Hiển thị Game Tip ngẫu nhiên
            DisplayRandomTip();
            SetLoadingUIActive(true);

            if (progressBar != null) progressBar.value = 0f;
            if (loadingText != null) loadingText.text = "ĐANG TẢI... 0%";

            // Khôi phục TimeScale về 1f để scene mới chạy chuẩn xác
            Time.timeScale = 1f;

            // 2. Load Scene bất đồng bộ (Async) kết hợp cập nhật Tiến trình & Spinner
            float startTime = Time.unscaledTime;
            float clampedMinLoadingTime = Mathf.Clamp(minimumLoadingTime, 0.2f, 2.0f);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            asyncLoad.allowSceneActivation = false;

            float currentProgress = 0f;
            while (asyncLoad.progress < 0.9f || (Time.unscaledTime - startTime < clampedMinLoadingTime))
            {
                float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.unscaledDeltaTime * 2.5f);

                if (progressBar != null) progressBar.value = currentProgress;
                if (loadingText != null) loadingText.text = $"ĐANG TẢI... {Mathf.RoundToInt(currentProgress * 100f)}%";

                if (activeSpinnerImage != null)
                {
                    activeSpinnerImage.transform.Rotate(0f, 0f, -spinnerRotateSpeed * Time.unscaledDeltaTime);
                }

                yield return null;
            }

            // Tiến trình hoàn tất 100%
            if (progressBar != null) progressBar.value = 1f;
            if (loadingText != null) loadingText.text = "ĐANG TẢI... 100%";

            yield return new WaitForSecondsRealtime(0.1f);

            // Cho phép kích hoạt scene mới
            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Đảm bảo có EventSystem trong Scene mới cho UI
            EnsureEventSystemInCurrentScene();

            SetLoadingUIActive(false);

            // 3. Cập nhật highestRoomReached & Auto Save khi chuyển tới Scene mới
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                int roomNumber = currentSequenceIndex + 1;
                if (roomNumber > SaveManager.Instance.CurrentSaveData.progressData.highestRoomReached)
                {
                    SaveManager.Instance.CurrentSaveData.progressData.highestRoomReached = roomNumber;
                }
            }

            if (autoSaveOnArrival && SaveManager.Instance != null)
            {
                Debug.Log($"[SceneTransitionManager] Đã tới scene '{targetSceneName}', kích hoạt Auto-Save...");
                SaveManager.Instance.TriggerAutoSave(0.2f);
            }

            // 4. Fade In màn hình (Sáng dần scene mới)
            elapsed = 0f;
            float actualFadeInDuration = Mathf.Max(fadeInDuration, 0.1f);
            while (elapsed < actualFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (faderCanvasGroup != null)
                {
                    faderCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / actualFadeInDuration));
                }
                yield return null;
            }

            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
                faderCanvasGroup.blocksRaycasts = false;
                faderCanvasGroup.interactable = false;
                faderCanvasGroup.gameObject.SetActive(false); // Tự động Disable sau khi chuyển cảnh xong
            }

            IsTransitioning = false;
            onComplete?.Invoke();
        }

        private void EnsureEventSystemInCurrentScene()
        {
            if (EventSystem.current == null)
            {
                EventSystem existing = FindObjectOfType<EventSystem>();
                if (existing == null)
                {
                    GameObject esObj = new GameObject("EventSystem");
                    esObj.AddComponent<EventSystem>();
                    esObj.AddComponent<StandaloneInputModule>();
                    Debug.Log("[SceneTransitionManager] Đã tự động tạo EventSystem cho Scene mới.");
                }
            }
        }

        /// <summary>
        /// Tự động chuyển sang Scene kế tiếp theo danh sách tịnh tiến tuyến tính trong sceneSequence
        /// </summary>
        public void TransitionToNextInSequence(bool autoSaveOnArrival = true, Action onComplete = null)
        {
            if (sceneSequence == null || sceneSequence.Length == 0)
            {
                Debug.LogError("[SceneTransitionManager] ❌ LỖI: Chưa cấu hình danh sách sceneSequence!");
                return;
            }

            string activeSceneName = SceneManager.GetActiveScene().name;

            if (currentSequenceIndex < sceneSequence.Length &&
                string.Equals(sceneSequence[currentSequenceIndex], activeSceneName, StringComparison.OrdinalIgnoreCase))
            {
                currentSequenceIndex = (currentSequenceIndex + 1) % sceneSequence.Length;
            }
            else
            {
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
            Debug.Log("[SceneTransitionManager] Đã đặt lại Sequence Index về 0.");
        }

        /// <summary>
        /// Gán thủ công sequence index
        /// </summary>
        public void SetSequenceIndex(int index)
        {
            if (sceneSequence != null && sceneSequence.Length > 0)
            {
                currentSequenceIndex = Mathf.Clamp(index, 0, sceneSequence.Length - 1);
            }
        }
    }
}
