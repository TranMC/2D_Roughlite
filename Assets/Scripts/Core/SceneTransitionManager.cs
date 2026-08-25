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
    /// Quản lý chuyển Scene mượt mà (Fade Screen, Load Async, Progress Bar, Tips ngẫu nhiên, Auto Save)
    /// Version: 1.4.2
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public const string VERSION = "1.4.2";
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
            "Mẹo: Có một số môi trường ngoại vi có thể gây sát thương, hãy cẩn thận",
            "Mẹo: Đừng quên ghé thăm Shop để hồi phục thể lực và sắm vũ khí mới.",
            "Mẹo: Mỗi loại kẻ địch có sơ hở riêng, hãy quan sát chuyển động của chúng.",
            "Mẹo: Thu thập Coin để nâng cấp chỉ số vĩnh viễn cho nhân vật.",
            "Mẹo: Quản lý Perk phù hợp cho mỗi lượt chơi!"
        };

        [Header("Sequence Settings")]
        [Tooltip("Thứ tự danh sách các Scene trong lượt chơi. Ví dụ: Scene1 -> ShopScene -> Scene2 -> ShopScene -> BossScene")]
        [SerializeField] private string[] sceneSequence = new string[] { "Scene1", "ShopScene", "Scene2", "ShopScene", "BossScene" };
        [SerializeField] private int currentSequenceIndex = 0;

        public bool IsTransitioning { get; private set; }

        private Image activeSpinnerImage;

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
                    canvas.sortingOrder = 999;

                    canvasObj.AddComponent<CanvasScaler>();
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
                    rect.offsetMax = Vector2.one;

                    // Container cho Loading Elements
                    GameObject loadingPanelObj = new GameObject("LoadingPanel");
                    loadingPanelObj.transform.SetParent(canvasObj.transform, false);
                    RectTransform loadingRect = loadingPanelObj.AddComponent<RectTransform>();
                    loadingRect.anchorMin = Vector2.zero;
                    loadingRect.anchorMax = Vector2.one;
                    loadingRect.offsetMin = Vector2.zero;
                    loadingRect.offsetMax = Vector2.one;

                    // Text Game Tip (Ở giữa / Phía dưới)
                    GameObject tipObj = new GameObject("TipText");
                    tipObj.transform.SetParent(loadingPanelObj.transform, false);
                    tipText = tipObj.AddComponent<TextMeshProUGUI>();
                    tipText.alignment = TextAlignmentOptions.Center;
                    tipText.fontSize = 22;
                    tipText.color = new Color(0.9f, 0.9f, 0.95f, 1f);
                    RectTransform tipRect = tipObj.GetComponent<RectTransform>();
                    tipRect.anchorMin = new Vector2(0.1f, 0.18f);
                    tipRect.anchorMax = new Vector2(0.9f, 0.28f);
                    tipRect.offsetMin = Vector2.zero;
                    tipRect.offsetMax = Vector2.one;

                    // Text Loading % (Ở góc phải dưới)
                    GameObject textObj = new GameObject("LoadingText");
                    textObj.transform.SetParent(loadingPanelObj.transform, false);
                    loadingText = textObj.AddComponent<TextMeshProUGUI>();
                    loadingText.alignment = TextAlignmentOptions.Right;
                    loadingText.fontSize = 24;
                    loadingText.fontStyle = FontStyles.Bold;
                    loadingText.color = Color.white;
                    RectTransform textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = new Vector2(0.6f, 0.08f);
                    textRect.anchorMax = new Vector2(0.9f, 0.15f);
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.one;

                    // Progress Bar (Slider)
                    GameObject sliderObj = new GameObject("ProgressBar");
                    sliderObj.transform.SetParent(loadingPanelObj.transform, false);
                    progressBar = sliderObj.AddComponent<Slider>();
                    RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
                    sliderRect.anchorMin = new Vector2(0.1f, 0.05f);
                    sliderRect.anchorMax = new Vector2(0.9f, 0.07f);
                    sliderRect.offsetMin = Vector2.zero;
                    sliderRect.offsetMax = Vector2.one;

                    // Fill Background
                    GameObject bgObj = new GameObject("Background");
                    bgObj.transform.SetParent(sliderObj.transform, false);
                    Image bgImage = bgObj.AddComponent<Image>();
                    bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
                    RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                    bgRect.anchorMin = Vector2.zero;
                    bgRect.anchorMax = Vector2.one;
                    bgRect.offsetMin = Vector2.zero;
                    bgRect.offsetMax = Vector2.one;

                    // Fill Area & Fill Image
                    GameObject fillObj = new GameObject("Fill");
                    fillObj.transform.SetParent(sliderObj.transform, false);
                    Image fillImage = fillObj.AddComponent<Image>();
                    fillImage.color = new Color(0.2f, 0.75f, 1f, 1f);
                    RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                    fillRect.anchorMin = Vector2.zero;
                    fillRect.anchorMax = Vector2.one;
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.one;

                    progressBar.fillRect = fillRect;
                    progressBar.minValue = 0f;
                    progressBar.maxValue = 1f;
                    progressBar.value = 0f;

                    // Dynamic Spinner Icon Setup
                    GameObject spinnerObj = new GameObject("SpinnerIcon");
                    spinnerObj.transform.SetParent(loadingPanelObj.transform, false);
                    activeSpinnerImage = spinnerObj.AddComponent<Image>();

                    RectTransform spinnerRect = spinnerObj.GetComponent<RectTransform>();
                    spinnerRect.anchorMin = new Vector2(0.07f, 0.045f);
                    spinnerRect.anchorMax = new Vector2(0.095f, 0.075f);
                    spinnerRect.offsetMin = Vector2.zero;
                    spinnerRect.offsetMax = Vector2.one;
                }
            }

            ResolveSpinnerReference();

            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
                faderCanvasGroup.blocksRaycasts = false;
            }

            SetLoadingUIActive(false);
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
        /// Chuyển tới Scene mới mượt mà với hiệu ứng Fade mờ dần, thanh Loading, Game Tip ngẫu nhiên và Auto-Save
        /// </summary>
        public void TransitionToScene(string targetSceneName, bool autoSaveOnArrival = true, Action onComplete = null)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Đang trong quá trình chuyển scene!");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"[SceneTransitionManager] ❌ LỖI CHUYỂN SCENE: Scene '{targetSceneName}' KHÔNG TỒN TẠI hoặc chưa được thêm vào 'File -> Build Settings'! (Index trong Sequence: {currentSequenceIndex})");
#if UNITY_EDITOR
                Debug.Break();
#endif
                return;
            }

            StartCoroutine(DoSceneTransition(targetSceneName, autoSaveOnArrival, onComplete));
        }

        private IEnumerator DoSceneTransition(string targetSceneName, bool autoSaveOnArrival, Action onComplete)
        {
            IsTransitioning = true;
            EnsureFaderSetup();

            faderCanvasGroup.blocksRaycasts = true;

            // 1. Fade Out màn hình (Tối dần)
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                faderCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
            faderCanvasGroup.alpha = 1f;

            // Kích hoạt giao diện Loading + Hiển thị Game Tip ngẫu nhiên
            DisplayRandomTip();
            SetLoadingUIActive(true);

            if (progressBar != null) progressBar.value = 0f;
            if (loadingText != null) loadingText.text = "ĐANG TẢI... 0%";

            // 2. Load Scene bất đồng bộ (Async) kết hợp cập nhật Tiến trình (Progress Bar) & Spinner
            float startTime = Time.unscaledTime;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            asyncLoad.allowSceneActivation = false;

            float currentProgress = 0f;
            while (asyncLoad.progress < 0.9f || (Time.unscaledTime - startTime < minimumLoadingTime))
            {
                // asyncLoad.progress chạy từ 0 -> 0.9
                float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.unscaledDeltaTime * 2f);

                if (progressBar != null) progressBar.value = currentProgress;
                if (loadingText != null) loadingText.text = $"ĐANG TẢI... {Mathf.RoundToInt(currentProgress * 100f)}%";

                // Xoay Spinner Icon
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

            SetLoadingUIActive(false);

            // 3. Auto Save khi chuyển tới Scene mới (nếu được yêu cầu)
            if (autoSaveOnArrival && SaveManager.Instance != null)
            {
                Debug.Log($"[SceneTransitionManager] Đã tới scene '{targetSceneName}', kích hoạt Auto-Save...");
                SaveManager.Instance.TriggerAutoSave(0.2f);
            }

            // 4. Fade In màn hình (Sáng dần scene mới)
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
        }
    }
}

