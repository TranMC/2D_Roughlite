using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text keybindText;

    private float cooldownDuration;
    private float currentCooldownTimer;
    private bool isCoolingDown = false;

    private void Update()
    {
        if (!isCoolingDown) return;

        currentCooldownTimer -= Time.deltaTime;

        if (currentCooldownTimer <= 0f)
        {
            // Kết thúc Cooldown
            ResetCooldown();
        }
        else
        {
            // Đang Cooldown: Cập nhật Radial Fill & Text số giây
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = currentCooldownTimer / cooldownDuration;
            }

            if (cooldownText != null)
            {
                // Hiện số nguyên nếu > 1s, hiện 1 chữ số thập phân nếu < 1s (ví dụ: 0.5s)
                cooldownText.text = currentCooldownTimer > 1f 
                    ? Mathf.CeilToInt(currentCooldownTimer).ToString() 
                    : currentCooldownTimer.ToString("F1");
            }
        }
    }

    // Hàm thiết lập Icon và Nút bấm ban đầu
    public void SetupSlot(Sprite icon, string keybind)
    {
        if (weaponIcon != null)
        {
            weaponIcon.sprite = icon;
            weaponIcon.gameObject.SetActive(icon != null);
        }

        if (keybindText != null)
        {
            keybindText.text = keybind;
        }

        ResetCooldown();
    }

    // Hàm gọi khi tung chiêu / đánh vũ khí để kích hoạt Cooldown
    public void TriggerCooldown(float duration)
    {
        if (duration <= 0f) return;

        cooldownDuration = duration;
        currentCooldownTimer = duration;
        isCoolingDown = true;

        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 1f;
    }

    // Reset trạng thái Cooldown về 0
    public void ResetCooldown()
    {
        isCoolingDown = false;
        currentCooldownTimer = 0f;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = "";
    }
}