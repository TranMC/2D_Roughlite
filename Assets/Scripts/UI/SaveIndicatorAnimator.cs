using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Roguelite.UI
{
    /// <summary>
    /// Script con (Component) chuyên quản lý hiệu ứng hoạt hoạ (Animation FX) cho UI Save Indicator.
    /// Version: 1.1.0
    /// </summary>
    public class SaveIndicatorAnimator : MonoBehaviour
    {
        public const string VERSION = "1.1.0";

        [Header("Target References")]
        [Tooltip("Icon lưu (floppy disk / save icon) để thực hiện xoay và nảy scale")]
        [SerializeField] private RectTransform iconTransform;

        [Tooltip("Text hiển thị trạng thái (TMP)")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Tooltip("CanvasGroup điều khiển mờ dần (Fade In/Out)")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.4f;
        [SerializeField] private float completionHoldDuration = 0.6f;

        [Header("Icon Animation Settings")]
        [SerializeField] private bool enableIconRotation = true;
        [SerializeField] private float rotationSpeed = -180f; // Độ/giây (âm là quay chiều kim đồng hồ)
        
        [SerializeField] private bool enableIconPulse = true;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float pulseScaleAmount = 0.12f; // Scale dao động từ (1 - amount) đến (1 + amount)

        [SerializeField] private bool enablePopOnStart = true;
        [SerializeField] private float popScaleMultiplier = 1.3f;
        [SerializeField] private float popDuration = 0.2f;

        [Header("Text Animation Settings")]
        [SerializeField] private bool animateDots = true;
        [SerializeField] private string baseText = "Đang lưu";
        [SerializeField] private float dotInterval = 0.3f;
        [SerializeField] private string completionText = "Đã lưu!";
        
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color completionTextColor = new Color(0.3f, 1f, 0.4f, 1f); // Xanh lá cây tươi

        // Runtime states
        private Coroutine fadeCoroutine;
        private Coroutine dotCoroutine;
        private Vector3 originalIconScale = Vector3.one;
        private Quaternion originalIconRotation = Quaternion.identity;
        private bool isSavingActive = false;

        private void Awake()
        {
            // Auto find components if not assigned
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (iconTransform == null)
            {
                Transform iconChild = transform.Find("SaveIcon");
                if (iconChild != null) iconTransform = iconChild.GetComponent<RectTransform>();
            }

            if (statusText == null)
            {
                statusText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (iconTransform != null)
            {
                originalIconScale = iconTransform.localScale;
                originalIconRotation = iconTransform.localRotation;
            }
        }

        /// <summary>
        /// Bắt đầu phát hiệu ứng Save Animation (Fade in, xoay icon, pulse scale, chấm động text)
        /// </summary>
        public void PlaySaveAnimation()
        {
            isSavingActive = true;
            gameObject.SetActive(true);

            // Reset text color
            if (statusText != null)
            {
                statusText.color = normalTextColor;
                statusText.text = baseText;
            }

            // Stop running routines
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if (dotCoroutine != null) StopCoroutine(dotCoroutine);

            // Fade in and Pop effect
            fadeCoroutine = StartCoroutine(DoFadeInAndPop());

            // Start Text dots animation
            if (animateDots && statusText != null)
            {
                dotCoroutine = StartCoroutine(DoAnimateDots());
            }
        }

        /// <summary>
        /// Dừng hiệu ứng Save với hiệu ứng hoàn tất ("Đã lưu!" + Fade out)
        /// </summary>
        public void StopSaveAnimation(bool success = true)
        {
            if (!gameObject.activeInHierarchy) return;

            isSavingActive = false;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if (dotCoroutine != null) StopCoroutine(dotCoroutine);

            fadeCoroutine = StartCoroutine(DoCompletionAndFadeOut(success));
        }

        private void Update()
        {
            if (!isSavingActive) return;

            // Icon Animation updates
            if (iconTransform != null)
            {
                // 1. Rotation (Quay xoay icon)
                if (enableIconRotation)
                {
                    iconTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
                }

                // 2. Pulse (Nhịp thở scale)
                if (enableIconPulse)
                {
                    float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseScaleAmount;
                    iconTransform.localScale = originalIconScale * (1f + pulse);
                }
            }
        }

        private IEnumerator DoFadeInAndPop()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            // Pop start scale
            Vector3 startScale = enablePopOnStart ? originalIconScale * popScaleMultiplier : originalIconScale;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                
                // Ease out quad for alpha
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t * (2f - t));

                if (enablePopOnStart && iconTransform != null)
                {
                    iconTransform.localScale = Vector3.Lerp(startScale, originalIconScale, t);
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator DoAnimateDots()
        {
            int dotCount = 0;
            while (isSavingActive)
            {
                if (statusText != null)
                {
                    string dots = new string('.', dotCount);
                    statusText.text = baseText + dots;
                }

                dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3 dots
                yield return new WaitForSeconds(dotInterval);
            }
        }

        private IEnumerator DoCompletionAndFadeOut(bool success)
        {
            // Hiệu ứng "Đã lưu!"
            if (statusText != null)
            {
                statusText.text = success ? completionText : "Lưu thất bại!";
                statusText.color = success ? completionTextColor : Color.red;
            }

            // Pop icon khi hoàn thành
            if (iconTransform != null)
            {
                iconTransform.localRotation = originalIconRotation; // Reset rotation về chuẩn
                float popElapsed = 0f;
                while (popElapsed < popDuration)
                {
                    popElapsed += Time.deltaTime;
                    float t = popElapsed / popDuration;
                    float scaleFactor = Mathf.Sin(t * Mathf.PI) * 0.25f;
                    iconTransform.localScale = originalIconScale * (1f + scaleFactor);
                    yield return null;
                }
                iconTransform.localScale = originalIconScale;
            }

            // Giữ màn hình vài giây để người chơi kịp nhìn thấy "Đã lưu!"
            yield return new WaitForSeconds(completionHoldDuration);

            // Fade Out mượt mà
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t * t); // Ease in quad fade out
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            isSavingActive = false;
            if (iconTransform != null)
            {
                iconTransform.localScale = originalIconScale;
                iconTransform.localRotation = originalIconRotation;
            }
        }
    }
}
