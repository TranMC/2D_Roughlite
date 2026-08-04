using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Roguelite.SaveSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// UI Popup hiển thị cảnh báo cho người chơi khi phát hiện file save bị hỏng hoặc đã được reset.
    /// </summary>
    public class SaveCorruptWarningUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private TextMeshProUGUI warningText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideWarning);
            }

            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            SaveManager.OnSaveCorruptDetected += HandleSaveCorruptDetected;
        }

        private void OnDisable()
        {
            SaveManager.OnSaveCorruptDetected -= HandleSaveCorruptDetected;
        }

        private void HandleSaveCorruptDetected(string message)
        {
            ShowWarning(message);
        }

        public void ShowWarning(string message)
        {
            if (warningText != null)
            {
                warningText.text = message;
            }

            if (warningPanel != null)
            {
                warningPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[SaveCorruptWarningUI] {message}");
            }
        }

        public void HideWarning()
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }
        }
    }
}
