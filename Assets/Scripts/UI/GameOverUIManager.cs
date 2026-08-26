using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Core;
using Roguelite.SaveSystem;
using Roguelite.UpgradeSystem;
using Roguelite.Combat;

namespace Roguelite.UI
{
    /// <summary>
    /// Quản lý giao diện Báo Cáo Kết Thúc Lượt Chạy (Run Summary / Game Over / Victory).
    /// Hiển thị chi tiết số liệu thống kê lượt chơi (Vàng, Quái diệt, Chiều sâu phòng, Perk, Vũ khí)
    /// và cung cấp nút Chạy Lượt Mới / Về Menu chính.
    /// </summary>
    public class GameOverUIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI currencySummaryText;
        
        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (gameOverPanel == null)
            {
                gameOverPanel = gameObject;
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            // Mặc định ẩn giao diện khi chưa vào GameOver hoặc Victory
            if (GameManager.Instance == null || (GameManager.Instance.CurrentState != GameState.GameOver && GameManager.Instance.CurrentState != GameState.Victory))
            {
                HideGameOverUI();
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                ShowRunSummaryUI(isVictory: false);
            }
            else if (state == GameState.Victory)
            {
                ShowRunSummaryUI(isVictory: true);
            }
            else
            {
                HideGameOverUI();
            }
        }

        /// <summary>
        /// Hiển thị bảng tổng kết lượt chạy với đầy đủ thông số chi tiết.
        /// </summary>
        /// <param name="isVictory">True nếu thắng WorldBoss, False nếu Player bị hạ gục</param>
        private void ShowRunSummaryUI(bool isVictory)
        {
            // Đóng bảng chọn Perk nếu đang mở để tránh block Raycast/UI
            if (RewardSelectionController.IsSelectionOpen)
            {
                RewardSelectionController.HideForPauseMenu();
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // 1. Cập nhật Tiêu Đề theo kết quả
            if (titleText != null)
            {
                if (isVictory)
                {
                    titleText.text = "<color=#00ff88>Chiến Thắng (VICTORY)!</color>\n<size=65%><color=#00e5ff>Hoàn thành lượt chạy xuất sắc</color></size>";
                }
                else
                {
                    titleText.text = "<color=#ff4d4d>Thất Bại (GAME OVER)</color>\n<size=65%><color=#aaaaaa>Kết thúc lượt chạy</color></size>";
                }
            }

            // 2. Thu thập và định dạng dữ liệu thống kê chi tiết
            if (currencySummaryText != null)
            {
                var progress = SaveManager.Instance?.CurrentSaveData?.progressData;
                int totalCurrency = progress != null ? progress.totalCurrency : 0;
                int kills = progress != null ? progress.totalEnemiesKilled : 0;
                int runs = progress != null ? progress.totalRunsPlayed : 0;
                int highestRoom = progress != null ? progress.highestRoomReached : 0;

                int perksCount = UpgradeManager.Instance != null && UpgradeManager.Instance.ActivePerks != null 
                    ? UpgradeManager.Instance.ActivePerks.Count : 0;

                int equippedWeapons = SaveManager.Instance?.CurrentSaveData?.weaponData?.equippedWeaponIds != null 
                    ? SaveManager.Instance.CurrentSaveData.weaponData.equippedWeaponIds.Count : 0;

                string statusLine = isVictory 
                    ? "<b>Kết quả:</b> <color=#00ff88>Đã hạ gục World Boss!</color>" 
                    : "<b>Kết quả:</b> <color=#ff4d4d>Bị đánh bại trên đường đi</color>";

                currencySummaryText.text = 
                    $"{statusLine}\n" +
                    $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                    $"<b>Tổng quái đã diệt:</b> <color=#00e5ff>{kills}</color>\n" +
                    $"<b>Phòng sâu nhất đạt được:</b> <color=#ffcc00>Phòng {highestRoom}</color>\n" +
                    $"<b>Lượt chạy thứ:</b> <color=#ffffff>#{runs}</color>\n" +
                    $"<b>Số Perk đã thu thập:</b> <color=#00ff88>{perksCount} Perks</color>\n" +
                    $"<b>Vũ khí Support hỗ trợ:</b> <color=#00e5ff>{equippedWeapons}/{WeaponUnlockData.MAX_EQUIPPED_SLOTS} Slots</color>\n" +
                    $"<b>Tổng vàng sở hữu:</b> <color=#ffcc00><b>{totalCurrency} Vàng</b></color>";
            }

            // Cập nhật text nút bấm
            if (retryButton != null)
            {
                var btnText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = isVictory ? "Chạy Lượt Mới" : "Chơi Lại";
                }
            }

            // Mở khóa con trỏ chuột để người chơi thao tác nút bấm UI
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Đảm bảo PlayerInput chuyển sang ActionMap UI để InputSystemUIInputModule nhận diện chuột / bàn phím
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SwitchPlayerInputActionMap("UI");
            }
            else
            {
                var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsInactive.Include);
                playerInput?.SwitchCurrentActionMap("UI");
            }

            if (UnityEngine.EventSystems.EventSystem.current != null && retryButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
            }
        }

        private void HideGameOverUI()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        public void OnRetryClicked()
        {
            HideGameOverUI();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRun();
            }
        }

        public void OnMainMenuClicked()
        {
            HideGameOverUI();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BackToMainMenu();
            }
        }
    }
}
