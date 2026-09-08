using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Roguelite.Core;
using Roguelite.Player;

namespace Roguelite.UI
{
    /// <summary>
    /// Tự động kiểm tra và đảm bảo các thành phần HUD (PlayerHealthBar, GoldDisplayUI) luôn tồn tại
    /// khi chuyển qua các Scene Gameplay (bao gồm cả ShopScene), tránh bị mất UI khi Scene trước bị Unload.
    /// Version: 1.0.0
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public const string VERSION = "1.0.0";
        public static HUDManager Instance { get; private set; }

        private const string HEALTH_BAR_PREFAB_RESOURCES = "Prefabs/PlayerHealthBar";
        private const string HEALTH_BAR_PREFAB_ROOT = "PlayerHealthBar";
        private const string HEALTH_BAR_PREFAB_ASSET_PATH = "Assets/Prefabs/Objects/PlayerHealthBar.prefab";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Kiểm tra và khởi tạo ngay cho Scene hiện tại khi vừa bắt đầu chạy
            EnsureHUDForScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureHUDForScene(scene);
        }

        /// <summary>
        /// Đảm bảo các thành phần HUD (Canvas, PlayerHealthBar, GoldDisplayUI) tồn tại trong Scene gameplay.
        /// </summary>
        public static void EnsureHUDForScene(Scene scene)
        {
            if (IsMainMenuScene(scene))
            {
                return;
            }

            Canvas hudCanvas = GetOrCreateHudCanvas(scene);
            if (hudCanvas == null)
            {
                Debug.LogWarning($"[HUDManager] Không thể tạo hoặc tìm thấy HUD Canvas cho scene: {scene.name}");
                return;
            }

            EnsurePlayerHealthBar(hudCanvas);
            EnsureGoldDisplay(hudCanvas);
        }

        /// <summary>
        /// Tìm Canvas HUD hiện có trong Scene hoặc tự động tạo "Main Canvas" mới nếu Scene chưa có.
        /// </summary>
        public static Canvas GetOrCreateHudCanvas(Scene scene)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Canvas namedMain = null;
            Canvas overlay = null;

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                {
                    continue;
                }

                // Bỏ qua Canvas chuyển cảnh fader
                if (canvas.name.Contains("Transition") || canvas.GetComponentInParent<SceneTransitionManager>() != null)
                {
                    continue;
                }

                // Ưu tiên Canvas thuộc chính scene đang xét
                if (canvas.gameObject.scene == scene)
                {
                    if (canvas.name == "Main Canvas")
                    {
                        namedMain = canvas;
                        break;
                    }

                    if (overlay == null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        overlay = canvas;
                    }
                }
            }

            if (namedMain != null) return namedMain;
            if (overlay != null) return overlay;

            // Tự động tạo "Main Canvas" cho scene nếu chưa có
            GameObject canvasGO = new GameObject("Main Canvas");
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(canvasGO, scene);
            }

            Canvas newCanvas = canvasGO.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            return newCanvas;
        }

        private static void EnsurePlayerHealthBar(Canvas hudCanvas)
        {
            PlayerHealthBar existing = FindFirstObjectByType<PlayerHealthBar>(FindObjectsInactive.Include);
            if (existing != null)
            {
                if (!existing.gameObject.activeSelf)
                {
                    existing.gameObject.SetActive(true);
                }
                existing.EnsureHudPlacement();
                existing.BindPlayer();
                return;
            }

            // Tìm prefab để instantiate
            GameObject prefab = Resources.Load<GameObject>(HEALTH_BAR_PREFAB_RESOURCES);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(HEALTH_BAR_PREFAB_ROOT);
            }

#if UNITY_EDITOR
            if (prefab == null)
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(HEALTH_BAR_PREFAB_ASSET_PATH);
            }
#endif

            GameObject healthBarGO;
            if (prefab != null)
            {
                healthBarGO = Instantiate(prefab, hudCanvas.transform, false);
                healthBarGO.name = "PlayerHealthBar";
            }
            else
            {
                // Fallback: Tạo thanh máu qua GameObject mới
                healthBarGO = new GameObject("PlayerHealthBar", typeof(RectTransform), typeof(Slider), typeof(PlayerHealthBar));
                healthBarGO.transform.SetParent(hudCanvas.transform, false);
            }

            PlayerHealthBar healthBar = healthBarGO.GetComponent<PlayerHealthBar>();
            if (healthBar != null)
            {
                healthBar.EnsureHudPlacement();
                healthBar.BindPlayer();
            }
        }

        private static void EnsureGoldDisplay(Canvas hudCanvas)
        {
            GoldDisplayUI.EnsureGoldUI(hudCanvas);
        }

        public static bool IsMainMenuScene(Scene scene)
        {
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.name))
            {
                string lower = scene.name.ToLower();
                if (lower.Contains("mainmenu") || lower.Contains("titlescreen") || lower.Contains("startmenu"))
                {
                    return true;
                }
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
    }
}
