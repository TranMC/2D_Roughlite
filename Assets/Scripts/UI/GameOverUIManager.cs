using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.Core;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Quản lý giao diện Game Over khi người chơi bị đánh bại.
    /// Hiển thị màn hình thua, thống kê số liệu lượt chơi và cung cấp nút Chơi lại / Về Menu chính.
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
            // Mặc định ẩn giao diện khi chưa vào Game Over
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.GameOver)
            {
                HideGameOverUI();
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                ShowGameOverUI();
            }
            else
            {
                HideGameOverUI();
            }
        }

        private void ShowGameOverUI()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = "GAME OVER";
            }

            if (currencySummaryText != null && SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                int totalCurrency = SaveManager.Instance.CurrentSaveData.progressData.totalCurrency;
                currencySummaryText.text = $"Tổng vàng sở hữu: {totalCurrency}";
            }

            // Mở khóa con trỏ chuột để người chơi thao tác nút bấm UI
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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
