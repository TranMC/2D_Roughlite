using UnityEngine;
using UnityEngine.UI;
using Roguelite.Core;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Điều khiển toàn bộ logic Main Menu UI.
    /// Quản lý các nút Start, Continue, Load, Quit và panel chọn Slot.
    /// (Nút Save chỉ có trong Pause Menu, không có ở Main Menu)
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("=== PANEL ===")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject slotPanel;

        [Header("=== NÚT CHÍNH ===")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button quitButton;

        [Header("=== NÚT TRONG SLOT PANEL ===")]
        [SerializeField] private Button backButton;
        [SerializeField] private SaveSlotUI[] slotUIs;

        // Chế độ hiện tại của Slot Panel
        private enum SlotPanelMode { Start, Load }
        private SlotPanelMode currentMode;

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
            startButton.onClick.AddListener(OnStartClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
            loadButton.onClick.AddListener(OnLoadClicked);
            quitButton.onClick.AddListener(OnQuitClicked);

            // Nút quay lại trong Slot Panel
            backButton.onClick.AddListener(OnBackClicked);

            // Mặc định: hiện Main Panel, ẩn Slot Panel
            ShowMainPanel();

            // Kiểm tra nút Continue có khả dụng không
            RefreshContinueButton();
        }

        // =====================================================================
        //  NÚT CHÍNH
        // =====================================================================

        /// <summary>START — Chọn slot để bắt đầu run mới.</summary>
        private void OnStartClicked()
        {
            currentMode = SlotPanelMode.Start;
            ShowSlotPanel();
        }

        /// <summary>CONTINUE — Tiếp tục từ slot cuối cùng đã chơi.</summary>
        private void OnContinueClicked()
        {
            if (SaveManager.Instance == null) return;

            int lastSlot = SaveManager.Instance.CurrentSlotIndex;

            // Kiểm tra slot cuối có dữ liệu không
            if (!SaveManager.Instance.DoesSlotExist(lastSlot))
            {
                Debug.LogWarning("[MainMenu] Không tìm thấy dữ liệu ở slot cuối cùng!");
                return;
            }

            SaveManager.Instance.SetCurrentSlot(lastSlot, autoLoad: true);
            Debug.Log($"[MainMenu] Tiếp tục từ Slot {lastSlot}.");
            GameManager.Instance.StartNewRun();
        }

        /// <summary>LOAD — Mở panel chọn slot ở chế độ Tải.</summary>
        private void OnLoadClicked()
        {
            currentMode = SlotPanelMode.Load;
            ShowSlotPanel();
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

            switch (currentMode)
            {
                case SlotPanelMode.Start:
                    HandleStartNewRun(slotIndex);
                    break;

                case SlotPanelMode.Load:
                    HandleLoadFromSlot(slotIndex);
                    break;
            }
        }

        /// <summary>Tạo save mới ở slot được chọn rồi bắt đầu run.</summary>
        private void HandleStartNewRun(int slotIndex)
        {
            SaveManager.Instance.SetCurrentSlot(slotIndex, autoLoad: false);
            SaveManager.Instance.SaveToDiskSync();
            Debug.Log($"[MainMenu] Bắt đầu run mới tại Slot {slotIndex}!");
            GameManager.Instance.StartNewRun();
        }

        /// <summary>Tải dữ liệu từ slot được chọn rồi vào game.</summary>
        private void HandleLoadFromSlot(int slotIndex)
        {
            if (!SaveManager.Instance.DoesSlotExist(slotIndex))
            {
                Debug.LogWarning($"[MainMenu] Slot {slotIndex} trống, không thể Load!");
                return;
            }

            SaveManager.Instance.SetCurrentSlot(slotIndex, autoLoad: true);
            Debug.Log($"[MainMenu] Đã tải Slot {slotIndex}. Vào game!");
            GameManager.Instance.StartNewRun();
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
            mainPanel.SetActive(true);
            slotPanel.SetActive(false);
        }

        private void ShowSlotPanel()
        {
            mainPanel.SetActive(false);
            slotPanel.SetActive(true);

            foreach (SaveSlotUI slot in slotUIs)
            {
                slot.Setup(OnSlotSelected);
            }
        }

        /// <summary>Bật/tắt nút Continue dựa trên việc có slot nào đã lưu chưa.</summary>
        private void RefreshContinueButton()
        {
            if (SaveManager.Instance == null)
            {
                continueButton.interactable = false;
                return;
            }

            bool hasAnySave = false;
            for (int i = SaveManager.MIN_SLOT_INDEX; i <= SaveManager.MAX_SLOT_INDEX; i++)
            {
                if (SaveManager.Instance.DoesSlotExist(i))
                {
                    hasAnySave = true;
                    break;
                }
            }

            continueButton.interactable = hasAnySave;
        }
    }
}
