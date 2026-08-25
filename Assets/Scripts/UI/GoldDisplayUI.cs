using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Core;
using Roguelite.UpgradeSystem;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Component UI hiển thị số tiền vàng (Currency/Gold) trên màn hình HUD.
    /// Hỗ trợ hiển thị Sprite/Image icon tiền vàng và Text số lượng.
    /// Tự động cập nhật khi số tiền thay đổi thông qua PermanentUpgradeManager và SaveManager.
    /// Version: 1.2.1
    /// </summary>
    public class GoldDisplayUI : MonoBehaviour
    {
        public const string VERSION = "1.2.1";

        [Header("UI References")]
        [SerializeField] private Image goldIconImage;
        [SerializeField] private TextMeshProUGUI goldTextTMP;
        [SerializeField] private Text goldText;

        [Header("Icon Settings")]
        [SerializeField] private Sprite goldIconSprite;
        [SerializeField] private Vector2 iconSize = new Vector2(32f, 32f);
        [SerializeField] private bool showIcon = true;

        [Header("Display Settings")]
        [SerializeField] private string prefix = "";
        [SerializeField] private string suffix = "";
        [SerializeField] private string numberFormat = "N0";
        [SerializeField] private bool useThousandsSeparator = true;
        [SerializeField] private bool showInMainMenu = false;

        [Header("HUD Placement")]
        [SerializeField] private bool lockToHUD = true;
        [SerializeField] private Vector2 anchorMin = new Vector2(0f, 1f); // Top Left HUD
        [SerializeField] private Vector2 anchorMax = new Vector2(0f, 1f);
        [SerializeField] private Vector2 pivot = new Vector2(0f, 1f);
        [SerializeField] private Vector2 hudOffset = new Vector2(36f, -36f);
        [SerializeField] private Vector2 hudSize = new Vector2(220f, 48f);

        private RectTransform rectTransform;
        private static Sprite generatedDefaultGoldSprite;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            EnsureUIElements();
        }

        private void OnEnable()
        {
            if (!showInMainMenu && IsMainMenuScene())
            {
                gameObject.SetActive(false);
                return;
            }

            EnsureHudPlacement();
            BindEvents();
            RefreshDisplay();
        }

        private void Start()
        {
            if (!showInMainMenu && IsMainMenuScene())
            {
                gameObject.SetActive(false);
                return;
            }

            EnsureHudPlacement();
            EnsureUIElements();
            RefreshDisplay();
        }

        private int lastObservedGold = -1;

        private void Update()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null && SaveManager.Instance.CurrentSaveData.progressData != null)
            {
                int currentGold = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
                if (currentGold != lastObservedGold)
                {
                    lastObservedGold = currentGold;
                    UpdateGoldDisplay(currentGold);
                }
            }
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void BindEvents()
        {
            UnbindEvents();
            PermanentUpgradeManager.OnCurrencyChanged += UpdateGoldDisplay;
            SaveManager.OnSaveCompleted += OnSaveCompleted;
        }

        private void UnbindEvents()
        {
            PermanentUpgradeManager.OnCurrencyChanged -= UpdateGoldDisplay;
            SaveManager.OnSaveCompleted -= OnSaveCompleted;
        }

        private void OnSaveCompleted()
        {
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            int currentGold = 0;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                currentGold = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
            }
            UpdateGoldDisplay(currentGold);
        }

        public void UpdateGoldDisplay(int totalGold)
        {
            if (!showInMainMenu && IsMainMenuScene())
            {
                return;
            }

            EnsureUIElements();

            string formattedNumber = useThousandsSeparator
                ? totalGold.ToString(numberFormat)
                : totalGold.ToString();

            string fullText = $"{prefix}{formattedNumber}{suffix}";

            if (goldTextTMP != null)
            {
                goldTextTMP.text = fullText;
            }

            if (goldText != null)
            {
                goldText.text = fullText;
            }
        }

        private void EnsureUIElements()
        {
            // 1. Quản lý Icon Image
            if (showIcon)
            {
                if (goldIconImage == null)
                {
                    goldIconImage = GetComponentInChildren<Image>();
                }

                if (goldIconImage == null)
                {
                    GameObject iconGO = new GameObject("GoldIconImage", typeof(RectTransform), typeof(Image));
                    iconGO.transform.SetParent(transform, false);
                    goldIconImage = iconGO.GetComponent<Image>();
                }

                RectTransform iconRect = goldIconImage.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = iconSize;

                if (goldIconSprite != null)
                {
                    goldIconImage.sprite = goldIconSprite;
                }
                else if (goldIconImage.sprite == null)
                {
                    goldIconImage.sprite = GetOrCreateDefaultGoldSprite();
                }

                goldIconImage.raycastTarget = false;
                goldIconImage.gameObject.SetActive(true);
            }
            else if (goldIconImage != null)
            {
                goldIconImage.gameObject.SetActive(false);
            }

            // 2. Quản lý Text Component
            if (goldTextTMP == null && goldText == null)
            {
                goldTextTMP = GetComponentInChildren<TextMeshProUGUI>();
                if (goldTextTMP == null)
                {
                    goldText = GetComponentInChildren<Text>();
                }
            }

            if (goldTextTMP == null && goldText == null)
            {
                GameObject textGO = new GameObject("Gold_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGO.transform.SetParent(transform, false);

                goldTextTMP = textGO.GetComponent<TextMeshProUGUI>();
                goldTextTMP.alignment = TextAlignmentOptions.MidlineLeft;
                goldTextTMP.fontSize = 24;
                goldTextTMP.color = new Color(1f, 0.85f, 0.2f); // Gold Yellow
                goldTextTMP.fontStyle = FontStyles.Bold;
                goldTextTMP.outlineWidth = 0.25f;
                goldTextTMP.outlineColor = Color.black;
                goldTextTMP.raycastTarget = false;
            }

            // Cập nhật khoảng offset cho Text để không đè lên Icon
            float textOffsetLeft = (showIcon && goldIconImage != null) ? (iconSize.x + 8f) : 0f;

            if (goldTextTMP != null)
            {
                RectTransform textRect = goldTextTMP.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(textOffsetLeft, 0f);
                textRect.offsetMax = Vector2.zero;
            }
            else if (goldText != null)
            {
                RectTransform textRect = goldText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(textOffsetLeft, 0f);
                textRect.offsetMax = Vector2.zero;
            }
        }

        private void EnsureHudPlacement()
        {
            if (!lockToHUD || rectTransform == null) return;

            Canvas hudCanvas = FindHudCanvas();
            if (hudCanvas != null && transform.parent != hudCanvas.transform)
            {
                transform.SetParent(hudCanvas.transform, false);
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
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

            if (namedMain != null) return namedMain;
            if (overlay != null) return overlay;
            return cameraCanvas;
        }

        private static bool IsMainMenuScene()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(sceneName) && (sceneName.Equals("MainMenu", System.StringComparison.OrdinalIgnoreCase) || sceneName.Equals("MainMenuScene", System.StringComparison.OrdinalIgnoreCase) || sceneName.ToLower().Contains("mainmenu")))
            {
                return true;
            }

            if (FindFirstObjectByType<MainMenuController>() != null || FindFirstObjectByType<MainMenuManager>() != null)
            {
                return true;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.MainMenu)
            {
                return true;
            }

            return false;
        }

        private static Sprite GetOrCreateDefaultGoldSprite()
        {
            if (generatedDefaultGoldSprite != null) return generatedDefaultGoldSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color goldCenter = new Color(1f, 0.85f, 0.15f, 1f);
            Color goldBorder = new Color(0.85f, 0.6f, 0.05f, 1f);
            Color goldInnerBorder = new Color(1f, 0.95f, 0.5f, 1f);

            float radius = size * 0.45f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (dist > radius)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > radius - 2.5f)
                    {
                        tex.SetPixel(x, y, goldBorder);
                    }
                    else if (dist > radius - 4.5f)
                    {
                        tex.SetPixel(x, y, goldInnerBorder);
                    }
                    else
                    {
                        tex.SetPixel(x, y, goldCenter);
                    }
                }
            }

            tex.Apply();
            generatedDefaultGoldSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return generatedDefaultGoldSprite;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoEnsureGoldUI()
        {
            if (IsMainMenuScene()) return;

            Canvas mainCanvas = FindHudCanvas();
            if (mainCanvas == null) return;

            if (FindFirstObjectByType<GoldDisplayUI>() == null)
            {
                GameObject goldGO = new GameObject("GoldDisplayUI", typeof(RectTransform), typeof(GoldDisplayUI));
                goldGO.transform.SetParent(mainCanvas.transform, false);
            }
        }
    }
}
