using UnityEngine;
using UnityEngine.UI;
using Roguelite.Core;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Điều khiển toàn bộ logic Main Menu UI.
    /// Quản lý các nút Start, Load, Quit và panel chọn Slot.
    /// Save Slots: Slot 0 = Auto Save (chỉ Load được), Slot 1-3 = Manual Save.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("=== PANEL ===")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject slotPanel;
        [Tooltip("Panel cài đặt (object PauseMenu). Nếu để trống sẽ tự tìm theo tên trong scene.")]
        [SerializeField] private GameObject pauseMenu;

        [Header("=== NÚT CHÍNH ===")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionButton; // Thay thế nút Load
        [SerializeField] private Button quitButton;

        [Header("=== NÚT TRONG SLOT PANEL ===")]
        [SerializeField] private Button backButton;
        [SerializeField] private SaveSlotUI[] slotUIs;

        // =====================================================================
        //  UNITY LIFECYCLE
        // =====================================================================

        private void Start()
        {
            // Đảm bảo đang ở trạng thái MainMenu
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.MainMenu);
            }

            // Đăng ký sự kiện cho các nút chính
            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (optionButton != null) optionButton.onClick.AddListener(OnOptionClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            // Nút quay lại trong Slot Panel
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            ResolvePauseMenu();
            BindPauseMenuCloseButtons();

            // Mặc định: hiện Main Panel, ẩn Slot Panel và PauseMenu
            ShowMainPanel();
        }

        // =====================================================================
        //  NÚT CHÍNH
        // =====================================================================

        /// <summary>START — Mở panel chọn slot. Gộp cả tính năng Start và Load.</summary>
        private void OnStartClicked()
        {
            ShowSlotPanel();
        }

        /// <summary>OPTION — Bật/tắt object PauseMenu.</summary>
        public void OnOptionClicked()
        {
            ResolvePauseMenu();
            if (pauseMenu == null)
            {
                Debug.LogWarning("[MainMenu] Chưa gán PauseMenu. Kéo object PauseMenu vào field Pause Menu trên MainMenuController.");
                return;
            }

            if (pauseMenu.activeSelf)
            {
                ClosePauseMenu();
                return;
            }

            OpenPauseMenu();
        }

        public void ClosePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }

            ShowMainPanel();
        }

        /// <summary>QUIT — Thoát game.</summary>
        private void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        // =====================================================================
        //  SLOT PANEL
        // =====================================================================

        /// <summary>Xử lý khi người chơi bấm chọn 1 slot.</summary>
        private void OnSlotSelected(int slotIndex)
        {
            if (SaveManager.Instance == null) return;

            // Nếu slot đã có dữ liệu -> Tải game (Load)
            if (SaveManager.Instance.DoesSlotExist(slotIndex))
            {
                SaveManager.Instance.SetCurrentSlot(slotIndex, autoLoad: true);
                Debug.Log($"[MainMenu] Đã tải Slot {slotIndex}. Vào game!");
                GameManager.Instance.StartNewRun();
            }
            else
            {
                // Nếu slot trống (chưa có dữ liệu) -> Tạo mới (Start)
                
                // Chặn không cho tạo mới đè lên slot Auto Save (nếu nó lỡ bị trống)
                if (slotIndex == SaveManager.AUTOSAVE_SLOT_INDEX)
                {
                    Debug.LogWarning("[MainMenu] Không thể tạo run mới trên Auto Save slot!");
                    return;
                }

                SaveManager.Instance.SetCurrentSlot(slotIndex, autoLoad: false);
                SaveManager.Instance.SaveToDiskSync();
                Debug.Log($"[MainMenu] Bắt đầu run mới tại Slot {slotIndex}!");
                GameManager.Instance.StartNewRun();
            }
        }

        /// <summary>Nút Quay lại — đóng Slot Panel, mở lại Main Panel.</summary>
        private void OnBackClicked()
        {
            ShowMainPanel();
        }

        // =====================================================================
        //  HIỂN THỊ UI
        // =====================================================================

        private void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (slotPanel != null) slotPanel.SetActive(false);
            if (pauseMenu != null) pauseMenu.SetActive(false);
        }

        private void ShowSlotPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (slotPanel != null) slotPanel.SetActive(true);
            if (pauseMenu != null) pauseMenu.SetActive(false);

            if (slotUIs == null) return;

            foreach (SaveSlotUI slot in slotUIs)
            {
                if (slot == null) continue;
                slot.Setup(OnSlotSelected);
                
                // Mọi slot đều có thể tương tác (Slot 0 AutoSave chỉ Load được nếu có dữ liệu)
                slot.SetInteractable(true);
            }
        }

        private void OpenPauseMenu()
        {
            if (slotPanel != null) slotPanel.SetActive(false);
            if (pauseMenu != null) pauseMenu.SetActive(true);
        }

        private void ResolvePauseMenu()
        {
            if (pauseMenu != null)
            {
                return;
            }

            pauseMenu = FindSceneObjectByName("PauseMenu");
        }

        private void BindPauseMenuCloseButtons()
        {
            if (pauseMenu == null)
            {
                return;
            }

            Button[] buttons = pauseMenu.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                string buttonName = button.gameObject.name;
                if (buttonName == "Back" || buttonName == "BackButton" || buttonName == "CloseButton")
                {
                    button.onClick.RemoveListener(ClosePauseMenu);
                    button.onClick.AddListener(ClosePauseMenu);
                }
            }
        }

        private GameObject FindSceneObjectByName(string objectName)
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindChildRecursive(roots[i].transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string objectName)
        {
            if (parent.name == objectName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
