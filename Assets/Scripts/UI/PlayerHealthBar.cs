using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Player;

namespace Roguelite.UI
{
    /// <summary>
    /// Thanh máu player cố định góc trái dưới màn hình (HUD), không bám theo đầu nhân vật.
    /// Có hỗ trợ hiển thị HP dạng chữ (ví dụ: 100/100).
    /// Version: 1.1.0
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        public const string VERSION = "1.1.0";

        [Header("Slider Settings")]
        [SerializeField] private Slider slider;
        [SerializeField] private PlayerStats playerStats;

        [Header("Text Display Settings")]
        [SerializeField] private TextMeshProUGUI hpTextTMP;
        [SerializeField] private Text hpText;
        [SerializeField] private string hpFormat = "{0}/{1}";
        [SerializeField] private bool showHpText = true;

        [Header("Placement Settings")]
        [SerializeField] private bool lockToBottomLeft = true;
        [SerializeField] private Vector2 hudSize = new Vector2(380f, 100f);
        [SerializeField] private Vector2 hudOffset = new Vector2(36f, 28f);

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }
            EnsureHpTextComponent();
        }

        private void OnEnable()
        {
            EnsureHudPlacement();
            BindPlayer();
        }

        private void Start()
        {
            EnsureHudPlacement();
            EnsureHpTextComponent();
            BindPlayer();
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHealth;
            }
        }

        private void BindPlayer()
        {
            if (playerStats == null)
            {
                playerStats = FindFirstObjectByType<PlayerStats>();
            }

            if (playerStats == null)
            {
                return;
            }

            playerStats.OnHealthChanged -= UpdateHealth;
            playerStats.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerStats.CurrentHealth, playerStats.MaxHealth);
        }

        private void UpdateHealth(float current, float max)
        {
            if (slider != null)
            {
                slider.maxValue = Mathf.Max(max, 1f);
                slider.value = Mathf.Clamp(current, 0f, slider.maxValue);
            }

            UpdateHpText(current, max);
        }

        private void UpdateHpText(float current, float max)
        {
            if (!showHpText)
            {
                return;
            }

            EnsureHpTextComponent();

            int currentHp = Mathf.Max(0, Mathf.CeilToInt(current));
            int maxHp = Mathf.Max(1, Mathf.CeilToInt(max));
            string formattedText = string.Format(hpFormat, currentHp, maxHp);

            if (hpTextTMP != null)
            {
                hpTextTMP.text = formattedText;
            }

            if (hpText != null)
            {
                hpText.text = formattedText;
            }
        }

        private void EnsureHpTextComponent()
        {
            if (!showHpText || hpTextTMP != null || hpText != null)
            {
                return;
            }

            hpTextTMP = GetComponentInChildren<TextMeshProUGUI>();
            if (hpTextTMP != null)
            {
                return;
            }

            hpText = GetComponentInChildren<Text>();
            if (hpText != null)
            {
                return;
            }

            // Tự động tạo Text child hiển thị HP nếu chưa gán trong Inspector
            Transform parentTransform = slider != null ? slider.transform : transform;
            GameObject textGO = new GameObject("HP_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(parentTransform, false);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            hpTextTMP = textGO.GetComponent<TextMeshProUGUI>();
            hpTextTMP.alignment = TextAlignmentOptions.Center;
            hpTextTMP.fontSize = 18;
            hpTextTMP.color = Color.white;
            hpTextTMP.fontStyle = FontStyles.Bold;
            hpTextTMP.outlineWidth = 0.2f;
            hpTextTMP.outlineColor = Color.black;
            hpTextTMP.raycastTarget = false;
        }

        private void EnsureHudPlacement()
        {
            if (!lockToBottomLeft || rectTransform == null)
            {
                return;
            }

            Canvas hudCanvas = FindHudCanvas();
            if (hudCanvas != null && transform.parent != hudCanvas.transform)
            {
                transform.SetParent(hudCanvas.transform, false);
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.sizeDelta = hudSize;
            rectTransform.anchoredPosition = hudOffset;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        private static Canvas FindHudCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Canvas namedMain = null;
            Canvas overlay = null;
            Canvas cameraCanvas = null;

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                {
                    continue;
                }

                if (canvas.name == "Main Canvas")
                {
                    namedMain = canvas;
                    break;
                }

                if (overlay == null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    overlay = canvas;
                }

                if (cameraCanvas == null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    cameraCanvas = canvas;
                }
            }

            if (namedMain != null)
            {
                return namedMain;
            }

            if (overlay != null)
            {
                return overlay;
            }

            return cameraCanvas;
        }
    }
}
