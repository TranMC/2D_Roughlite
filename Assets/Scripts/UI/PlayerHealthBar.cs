using UnityEngine;
using UnityEngine.UI;
using Roguelite.Player;

namespace Roguelite.UI
{
    /// <summary>
    /// Thanh máu player cố định góc trái dưới màn hình (HUD), không bám theo đầu nhân vật.
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private PlayerStats playerStats;
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
        }

        private void OnEnable()
        {
            EnsureHudPlacement();
            BindPlayer();
        }

        private void Start()
        {
            EnsureHudPlacement();
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
            if (slider == null)
            {
                return;
            }

            slider.maxValue = Mathf.Max(max, 1f);
            slider.value = Mathf.Clamp(current, 0f, slider.maxValue);
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
