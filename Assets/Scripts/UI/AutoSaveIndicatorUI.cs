using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// UI component hiển thị trạng thái auto-save với icon và text "Đang lưu..."
    /// Version: 1.2.0
    /// </summary>
    public class AutoSaveIndicatorUI : MonoBehaviour
    {
        public const string VERSION = "1.2.0";

        [Header("UI Elements")]
        [SerializeField] private GameObject indicatorPanel;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI saveText;
        
        [Header("Icon Settings")]
        [SerializeField] private Sprite saveIcon;
        
        [Header("Animation Settings")]
        [SerializeField] private SaveIndicatorAnimator animator;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float minAlpha = 0.5f;
        [SerializeField] private float maxAlpha = 1f;
        
        private CanvasGroup canvasGroup;
        private bool isAnimating;
        private float time;

        private void Awake()
        {
            Debug.Log("[AutoSaveIndicatorUI] Awake called");
            
            // Auto-assign if not set in Inspector
            if (indicatorPanel == null)
            {
                indicatorPanel = gameObject;
                Debug.Log("[AutoSaveIndicatorUI] Auto-assigned indicatorPanel to self: " + gameObject.name);
            }
            
            canvasGroup = indicatorPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = indicatorPanel.AddComponent<CanvasGroup>();
            }
            
            if (animator == null)
            {
                animator = GetComponent<SaveIndicatorAnimator>();
                if (animator == null)
                {
                    animator = GetComponentInChildren<SaveIndicatorAnimator>();
                }
            }

            Debug.Log("[AutoSaveIndicatorUI] indicatorPanel assigned: " + indicatorPanel.name);
            
            // Set initial state - chỉ ẩn UI elements, không disable panel
            HideIndicator();
        }

        private void OnEnable()
        {
            Debug.Log("[AutoSaveIndicatorUI] OnEnable - Registering events");
            SaveManager.OnAutoSavePending += ShowIndicator;
            SaveManager.OnSaveStarted += OnSaveStarted;
            SaveManager.OnSaveCompleted += HideIndicator;
        }

        private void OnDisable()
        {
            SaveManager.OnAutoSavePending -= ShowIndicator;
            SaveManager.OnSaveStarted -= OnSaveStarted;
            SaveManager.OnSaveCompleted -= HideIndicator;
        }

        private void Start()
        {
            // Set icon if assigned
            if (iconImage != null && saveIcon != null)
            {
                iconImage.sprite = saveIcon;
            }
            
            // Set text
            if (saveText != null && animator == null)
            {
                saveText.text = "Đang lưu...";
            }
        }

        private void ShowIndicator()
        {
            Debug.Log("[AutoSaveIndicatorUI] ShowIndicator called");
            
            if (animator != null)
            {
                animator.PlaySaveAnimation();
                return;
            }

            // Fallback nếu không gắn SaveIndicatorAnimator
            if (iconImage != null && iconImage.gameObject != null)
            {
                iconImage.gameObject.SetActive(true);
            }
            
            if (saveText != null && saveText.gameObject != null)
            {
                saveText.gameObject.SetActive(true);
            }
            
            if (indicatorPanel != null)
            {
                indicatorPanel.SetActive(true);
            }
            
            isAnimating = true;
            time = 0f;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void OnSaveStarted()
        {
            // Additional handling when save actually starts
            if (animator != null)
            {
                animator.PlaySaveAnimation();
            }
            else if (saveText != null)
            {
                saveText.text = "Đang lưu...";
            }
        }

        private void HideIndicator()
        {
            Debug.Log("[AutoSaveIndicatorUI] HideIndicator called");
            
            if (animator != null)
            {
                animator.StopSaveAnimation(true);
                return;
            }

            // Fallback nếu không có animator
            if (iconImage != null && iconImage.gameObject != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            
            if (saveText != null && saveText.gameObject != null)
            {
                saveText.gameObject.SetActive(false);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            
            isAnimating = false;
        }

        private void Update()
        {
            if (animator != null) return; // Đã xử lý hoạt hoạ trong SaveIndicatorAnimator

            if (isAnimating && canvasGroup != null)
            {
                time += Time.deltaTime * pulseSpeed;
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(time) + 1f) / 2f);
                canvasGroup.alpha = alpha;
            }
        }
        
        /// <summary>
        /// Thay đổi icon save
        /// </summary>
        public void SetSaveIcon(Sprite newIcon)
        {
            saveIcon = newIcon;
            if (iconImage != null)
            {
                iconImage.sprite = saveIcon;
            }
        }
        
        /// <summary>
        /// Thay đổi text hiển thị
        /// </summary>
        public void SetSaveText(string text)
        {
            if (saveText != null)
            {
                saveText.text = text;
            }
        }
        
        /// <summary>
        /// Test method để debug - gọi trực tiếp từ Inspector hoặc code
        /// </summary>
        [ContextMenu("Debug/Test Show Indicator")]
        public void DebugTestShow()
        {
            Debug.Log("[AutoSaveIndicatorUI] DebugTestShow called");
            ShowIndicator();
        }
        
        /// <summary>
        /// Test method để debug - gọi trực tiếp từ Inspector hoặc code
        /// </summary>
        [ContextMenu("Debug/Test Hide Indicator")]
        public void DebugTestHide()
        {
            Debug.Log("[AutoSaveIndicatorUI] DebugTestHide called");
            HideIndicator();
        }
    }
}