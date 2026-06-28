using UnityEngine;
using UnityEngine.InputSystem;
using Roguelite.Core;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject optionCanvas;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

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
    }

    /// <summary>Gọi từ Player Input event (Pause Menu). Chỉ xử lý 1 lần mỗi lần bấm phím.</summary>
    public void OnPauseInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        HandlePauseInput();
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

        ResumeGame();
    }

    public void OnContinueClicked() => ResumeGame();

    public void OnOptionClicked()
    {
        isShowingOptions = true;
        pauseCanvas.SetActive(false);
        optionCanvas.SetActive(true);
    }

    public void OnBackClicked() => ShowPauseMenu();

    private void OpenPauseMenu()
    {
        isPaused = true;
        isShowingOptions = false;
        pauseCanvas.SetActive(true);
        optionCanvas.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();

        playerInput.SwitchCurrentActionMap("UI");
    }

    private void ShowPauseMenu()
    {
        isShowingOptions = false;
        pauseCanvas.SetActive(true);
        optionCanvas.SetActive(false);
    }

    private void ResumeGame()
    {
        isPaused = false;
        isShowingOptions = false;
        pauseCanvas.SetActive(false);
        optionCanvas.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        playerInput.SwitchCurrentActionMap("Player");
    }
}
