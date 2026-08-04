using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// UI component hiển thị trạng thái auto-save với icon và text "Đang lưu..."
    /// </summary>
    public class AutoSaveIndicatorUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject indicatorPanel;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI saveText;
        
        [Header("Icon Settings")]
        [SerializeField] private Sprite saveIcon;
        
        [Header("Animation")]
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
            if (saveText != null)
            {
                saveText.text = "Đang lưu...";
            }
        }

        private void ShowIndicator()
        {
            Debug.Log("[AutoSaveIndicatorUI] ShowIndicator called");
            
            // Bật các UI elements con thay vì disable panel
            if (iconImage != null && iconImage.gameObject != null)
            {
                iconImage.gameObject.SetActive(true);
                Debug.Log("[AutoSaveIndicatorUI] Enabled iconImage");
            }
            
            if (saveText != null && saveText.gameObject != null)
            {
                saveText.gameObject.SetActive(true);
                Debug.Log("[AutoSaveIndicatorUI] Enabled saveText");
            }
            
            // Panel luôn enable, chỉ dùng canvasGroup để điều khiển alpha
            if (indicatorPanel != null)
            {
                indicatorPanel.SetActive(true);
            }
            
            isAnimating = true;
            time = 0f;
            
            // Set alpha để hiển thị
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void OnSaveStarted()
        {
            // Additional handling when save actually starts
            if (saveText != null)
            {
                saveText.text = "Đang lưu...";
            }
        }

        private void HideIndicator()
        {
            Debug.Log("[AutoSaveIndicatorUI] HideIndicator called");
            
            // Chỉ ẩn các UI elements con, không disable panel
            if (iconImage != null && iconImage.gameObject != null)
            {
                iconImage.gameObject.SetActive(false);
                Debug.Log("[AutoSaveIndicatorUI] Disabled iconImage");
            }
            
            if (saveText != null && saveText.gameObject != null)
            {
                saveText.gameObject.SetActive(false);
                Debug.Log("[AutoSaveIndicatorUI] Disabled saveText");
            }
            
            // Panel luôn enable, chỉ dùng canvasGroup alpha để ẩn
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            
            isAnimating = false;
        }

        private void Update()
        {
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