using UnityEngine;
using UnityEngine.InputSystem;
using Roguelite.Core;
using Roguelite.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject optionCanvas;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    public static bool IsMenuOpen { get; private set; }

    private static int lastPauseInputFrame = -1;

    private bool isPaused;
    private bool isShowingOptions;

    private void Awake()
    {
        // Tránh tham chiếu nhầm component trên prefab asset (SwitchCurrentActionMap sẽ không chạy trên instance trong scene).
        if (playerInput == null || !playerInput.gameObject.scene.IsValid())
            playerInput = FindFirstObjectByType<PlayerInput>();
    }

    private void Start()
    {
        pauseCanvas.SetActive(false);
        optionCanvas.SetActive(false);
        IsMenuOpen = false;
    }

    private void OnDestroy()
    {
        IsMenuOpen = false;
    }

    /// <summary>Gọi từ Player Input event (Pause Menu). Chỉ xử lý 1 lần mỗi lần bấm phím.</summary>
    public void OnPauseInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        RequestTogglePauseMenu();
    }

    /// <summary>Mở/đóng pause menu; dùng chung cho Player Input và Reward Selection (tránh double-trigger Escape).</summary>
    public static void RequestTogglePauseMenu()
    {
        if (Time.frameCount == lastPauseInputFrame) return;
        lastPauseInputFrame = Time.frameCount;

        var manager = FindFirstObjectByType<PauseMenuManager>();
        manager?.HandlePauseInput();
    }

    private void HandlePauseInput()
    {
        if (!isPaused)
        {
            OpenPauseMenu();
            return;
        }

        if (isShowingOptions)
        {
            ShowPauseMenu();
            return;
        }

        ClosePauseMenu();
    }

    public void OnContinueClicked() => ClosePauseMenu();

    public void OnOptionClicked()
    {
        isShowingOptions = true;
        pauseCanvas.SetActive(false);
        optionCanvas.SetActive(true);
    }

    public void OnBackClicked() => ShowPauseMenu();

    private void OpenPauseMenu()
    {
        if (RewardSelectionController.IsSelectionOpen)
            RewardSelectionController.HideForPauseMenu();

        isPaused = true;
        IsMenuOpen = true;
        isShowingOptions = false;
        pauseCanvas.SetActive(true);
        optionCanvas.SetActive(false);

        if (GameManager.Instance != null)
        {
            var state = GameManager.Instance.CurrentState;
            if (state == GameState.Gameplay || state == GameState.RewardSelection)
                GameManager.Instance.PauseGame();
        }

        playerInput.SwitchCurrentActionMap("UI");
    }

    private void ShowPauseMenu()
    {
        isShowingOptions = false;
        pauseCanvas.SetActive(true);
        optionCanvas.SetActive(false);
    }

    private void ClosePauseMenu()
    {
        isPaused = false;
        IsMenuOpen = false;
        isShowingOptions = false;
        pauseCanvas.SetActive(false);
        optionCanvas.SetActive(false);

        if (RewardSelectionController.IsSelectionOpen)
        {
            RewardSelectionController.RestoreAfterPauseMenu();
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        playerInput.SwitchCurrentActionMap("Player");
    }
}
