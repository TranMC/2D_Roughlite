using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Roguelite.Core;
using Roguelite.UpgradeSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Điều khiển bảng giao diện chọn Perk phần thưởng (Reward Selection Panel).
    /// Quản lý bật/tắt UI, random các Perk, và bắt sự kiện phím tắt 1-2-3 / click để chọn Perk.
    /// </summary>
    public class RewardSelectionController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Container Panel chính chứa UI chọn Perk để bật/tắt.")]
        [SerializeField] private GameObject selectionPanel;

        [Tooltip("Mảng chứa 3 RewardCardUI để hiển thị 3 lựa chọn.")]
        [SerializeField] private RewardCardUI[] rewardCards;

        [Header("Animation Settings")]
        [SerializeField] private float openAnimationDuration = 0.25f;
        [SerializeField] private float cardStaggerDelay = 0.08f;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;

        public static bool IsSelectionOpen { get; private set; }

        private static RewardSelectionController instance;
        public static RewardSelectionController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<RewardSelectionController>(FindObjectsInactive.Include);
                }
                return instance;
            }
        }

        private List<PerkData> currentOptions = new List<PerkData>();
        private bool isProcessingSelection = false;
        private Coroutine panelOpenCoroutine;

        private void Awake()
        {
            instance = this;

            if (playerInput == null || !playerInput.gameObject.scene.IsValid())
                playerInput = FindFirstObjectByType<PlayerInput>();

            if (selectionPanel != null && panelCanvasGroup == null)
            {
                panelCanvasGroup = selectionPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                    panelCanvasGroup = selectionPanel.AddComponent<CanvasGroup>();
            }

            // Awake chạy trước Start — tránh race khi OpenSelection() được gọi sớm (Context Menu)
            // rồi Start() tắt panel nhưng vẫn giữ input/timeScale ở trạng thái "đang mở".
            HideSelectionPanel();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        /// <summary>Ẩn panel reward tạm thời khi mở pause menu (vẫn giữ trạng thái chọn perk).</summary>
        public static void HideForPauseMenu()
        {
            if (!IsSelectionOpen || instance?.selectionPanel == null) return;
            instance.selectionPanel.SetActive(false);
        }

        /// <summary>Khôi phục panel reward sau khi đóng pause menu.</summary>
        public static void RestoreAfterPauseMenu()
        {
            if (!IsSelectionOpen || instance == null) return;

            instance.EnsurePanelVisible();
            instance.selectionPanel.SetActive(true);

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.EnterRewardSelection();
        }

        private void Update()
        {
            if (!IsSelectionOpen || isProcessingSelection) return;

            if (PauseMenuManager.IsMenuOpen)
                return;

            // UI/Cancel và UI/Pause Menu cùng dùng Escape — đảm bảo pause menu vẫn mở được khi panel đang hiện.
            if (Input.GetKeyDown(KeyCode.Escape) || Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                PauseMenuManager.RequestTogglePauseMenu();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ChoosePerk(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ChoosePerk(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ChoosePerk(2);
            }
        }

        [ContextMenu("Debug/Open Selection Panel")]
        public void OpenSelection()
        {
            RecoverFromInconsistentState();

            if (IsSelectionOpen)
            {
                Debug.LogWarning("[RewardSelectionController] Panel đang mở sẵn từ trước!");
                return;
            }

            if (PauseMenuManager.IsMenuOpen)
            {
                Debug.LogWarning("[RewardSelectionController] Không thể mở reward panel khi pause menu đang bật.");
                return;
            }

            if (selectionPanel == null || rewardCards == null || rewardCards.Length < 3)
            {
                Debug.LogError($"[RewardSelectionController] Cấu hình UI chưa đủ! selectionPanel: {(selectionPanel != null ? "Đã gán" : "Chưa gán (NULL)")}, mảng rewardCards: {(rewardCards != null ? rewardCards.Length.ToString() : "NULL")}");
                return;
            }

            if (UpgradeManager.Instance == null)
            {
                Debug.LogError("[RewardSelectionController] Không tìm thấy UpgradeManager Instance trong Scene!");
                return;
            }

            currentOptions = UpgradeManager.Instance.GetRandomPerks(3);

            if (currentOptions == null || currentOptions.Count == 0)
            {
                Debug.LogWarning("[RewardSelectionController] Không rút được Perk nào! Hãy kiểm tra PerkPool trên UpgradeManager có chứa danh sách PerkData chưa.");
                return;
            }

            EnsurePanelVisible();
            BindCardButtons();
            isProcessingSelection = false;

            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (i < currentOptions.Count)
                {
                    rewardCards[i].gameObject.SetActive(true);
                    rewardCards[i].SetupCard(currentOptions[i]);
                    rewardCards[i].PlayEntranceAnimation(i * cardStaggerDelay);
                }
                else
                {
                    rewardCards[i].gameObject.SetActive(false);
                }
            }

            selectionPanel.SetActive(true);

            if (!selectionPanel.activeInHierarchy)
            {
                Debug.LogError("[RewardSelectionController] Không thể hiển thị selection panel — kiểm tra hierarchy Canvas/parent.");
                return;
            }

            IsSelectionOpen = true;
            PauseForSelection();
            SoundManager.PlaySound(SoundType.PERK_SELECT);

            // Chạy animation mở panel bằng CanvasGroup và Scale
            if (panelOpenCoroutine != null) StopCoroutine(panelOpenCoroutine);
            panelOpenCoroutine = StartCoroutine(AnimateOpenPanelRoutine());

            Debug.Log("[RewardSelectionController] Đã mở bảng chọn Perk, trò chơi tạm dừng.");
        }

        private IEnumerator AnimateOpenPanelRoutine()
        {
            if (panelCanvasGroup == null) yield break;

            panelCanvasGroup.alpha = 0f;
            Vector3 startScale = Vector3.one * 0.88f;
            selectionPanel.transform.localScale = startScale;

            float elapsed = 0f;
            while (elapsed < openAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / openAnimationDuration);
                float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease out

                panelCanvasGroup.alpha = t;
                selectionPanel.transform.localScale = Vector3.Lerp(startScale, Vector3.one, smoothT);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
            selectionPanel.transform.localScale = Vector3.one;
            panelOpenCoroutine = null;
        }

        public void ChoosePerk(int index)
        {
            if (!IsSelectionOpen || isProcessingSelection) return;
            if (index < 0 || index >= currentOptions.Count) return;

            isProcessingSelection = true;
            StartCoroutine(ChoosePerkRoutine(index));
        }

        private IEnumerator ChoosePerkRoutine(int index)
        {
            if (index >= 0 && index < rewardCards.Length && rewardCards[index] != null)
            {
                rewardCards[index].TriggerClickEffect();
            }

            float delay = 0.08f;
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            PerkData chosenPerk = currentOptions[index];
            if (chosenPerk != null && UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.AddPerk(chosenPerk);
                Debug.Log($"[RewardSelectionController] Đã chọn Perk: {chosenPerk.PerkName}");
            }

            CloseSelection();
        }

        private void CloseSelection()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
                if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1f;
                selectionPanel.transform.localScale = Vector3.one;
            }

            IsSelectionOpen = false;
            isProcessingSelection = false;
            currentOptions.Clear();

            ResumeFromSelection();

            if (PauseMenuManager.IsMenuOpen)
                Debug.Log("[RewardSelectionController] Đã đóng bảng chọn Perk. Game vẫn tạm dừng vì pause menu đang bật.");
            else
                Debug.Log("[RewardSelectionController] Đã đóng bảng chọn Perk, trò chơi tiếp tục.");
        }

        private void HideSelectionPanel()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
                if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1f;
                selectionPanel.transform.localScale = Vector3.one;
            }

            IsSelectionOpen = false;
            isProcessingSelection = false;
        }

        /// <summary>
        /// Dọn trạng thái lỗi: panel ẩn nhưng game/input vẫn bị khóa từ lần mở trước.
        /// </summary>
        private void RecoverFromInconsistentState()
        {
            bool panelVisible = selectionPanel != null && selectionPanel.activeInHierarchy;

            if (IsSelectionOpen && !panelVisible)
            {
                IsSelectionOpen = false;
                ResumeFromSelection();
                return;
            }

            if (!IsSelectionOpen && !panelVisible)
                ResumeFromSelection();
        }

        private void PauseForSelection()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EnterRewardSelection();
            else
                Time.timeScale = 0f;

            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("UI");

            // Mở và hiện con trỏ chuột để tương tác với UI chọn Perk
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResumeFromSelection()
        {
            if (GameManager.Instance != null)
            {
                if (PauseMenuManager.IsMenuOpen)
                {
                    if (GameManager.Instance.CurrentState == GameState.RewardSelection)
                        GameManager.Instance.ChangeState(GameState.Paused);
                }
                else
                {
                    GameManager.Instance.ExitRewardSelection();
                }
            }
            else
            {
                Time.timeScale = PauseMenuManager.IsMenuOpen ? 0f : 1f;
            }

            if (PauseMenuManager.IsMenuOpen)
                return;

            if (playerInput != null && playerInput.currentActionMap != null &&
                playerInput.currentActionMap.name == "UI")
            {
                playerInput.SwitchCurrentActionMap("Player");
            }
        }

        private void EnsurePanelVisible()
        {
            Transform current = selectionPanel.transform;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                current = current.parent;
            }

            Canvas canvas = selectionPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
            }
        }

        private void BindCardButtons()
        {
            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (rewardCards[i] == null) continue;

                int index = i;
                Button button = rewardCards[i].GetComponent<Button>();
                if (button == null) continue;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ChoosePerk(index));
            }
        }
    }
}
