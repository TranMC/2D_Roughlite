using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roguelite.Core
{
    /// <summary>
    /// Các trạng thái chính của vòng đời trò chơi.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Quản lý trạng thái và luồng vận hành chính của trò chơi (Singleton).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Trạng thái Hiện tại")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        public GameState CurrentState => currentState;

        // Sự kiện thông báo khi trạng thái game thay đổi
        public static event Action<GameState> OnGameStateChanged;

        [Header("Cấu hình Tên Scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        [SerializeField] private string gameplaySceneName = "SampleScene";

        private void Awake()
        {
            // Thiết lập Singleton và giữ GameManager qua các Scene
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Tự động nhận diện trạng thái game dựa theo scene hiện tại đang active lúc chạy
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == mainMenuSceneName)
            {
                ChangeState(GameState.MainMenu);
            }
            else if (activeSceneName == gameplaySceneName)
            {
                ChangeState(GameState.Gameplay);
            }
        }

        /// <summary>
        /// Thay đổi trạng thái game và thực hiện các thiết lập hệ thống tương ứng (timeScale).
        /// </summary>
        /// <param name="newState">Trạng thái mới cần chuyển đổi</param>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState && newState != GameState.MainMenu) return;

            currentState = newState;
            Debug.Log($"[GameManager] Chuyển trạng thái sang: {currentState}");

            // Xử lý các logic toàn cục khi thay đổi trạng thái
            switch (currentState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f; // Đóng băng gameplay khi thua cuộc
                    break;
                case GameState.Victory:
                    Time.timeScale = 0f; // Đóng băng gameplay khi chiến thắng
                    break;
            }

            // Kích hoạt Event để các module khác nhận diện
            OnGameStateChanged?.Invoke(currentState);
        }

        // --- CÁC PHƯƠNG THỨC TIỆN ÍCH ĐIỀU PHỐI VÒNG LẶP CHƠI ---

        /// <summary>
        /// Bắt đầu một lượt chơi mới (chuyển sang gameplay).
        /// </summary>
        public void StartNewRun()
        {
            ChangeState(GameState.Gameplay);
            LoadScene(gameplaySceneName);
        }

        /// <summary>
        /// Tạm dừng trò chơi.
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Gameplay)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// Tiếp tục trò chơi.
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Gameplay);
            }
        }

        /// <summary>
        /// Khởi động lại lượt chơi hiện tại.
        /// </summary>
        public void RestartRun()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Gameplay);
            LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Quay lại Main Menu.
        /// </summary>
        public void BackToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Thoát trò chơi.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Đang thoát trò chơi...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // --- QUẢN LÝ TẢI SCENE ---

        /// <summary>
        /// Tải Scene an toàn với xử lý lỗi cơ bản.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] Không thể tải Scene '{sceneName}': {e.Message}");
            }
        }
    }
}
