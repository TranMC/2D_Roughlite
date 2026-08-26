using UnityEngine;

namespace Roguelite.BuffSystem
{
    /// <summary>
    /// Dữ liệu hiển thị một icon buff, bao gồm thời gian còn lại để xử lý nhấp nháy cảnh báo.
    /// </summary>
    public readonly struct BuffDisplayInfo
    {
        public BuffDefinition Definition { get; }
        public Sprite Icon { get; }
        public string BuffId { get; }
        public float RemainingTime { get; }
        public float TotalDuration { get; }

        public BuffDisplayInfo(BuffDefinition definition, float remainingTime, float totalDuration)
        {
            Definition = definition;
            Icon = definition != null ? definition.Icon : null;
            BuffId = definition != null ? definition.BuffType.ToString() : string.Empty;
            RemainingTime = remainingTime;
            TotalDuration = totalDuration > 0f ? totalDuration : remainingTime;
        }

        public BuffDisplayInfo(Sprite icon, float remainingTime, float totalDuration, string buffId = "", BuffDefinition definition = null)
        {
            Definition = definition;
            Icon = icon != null ? icon : (definition != null ? definition.Icon : null);
            BuffId = !string.IsNullOrEmpty(buffId) ? buffId : (definition != null ? definition.BuffType.ToString() : string.Empty);
            RemainingTime = remainingTime;
            TotalDuration = totalDuration > 0f ? totalDuration : remainingTime;
        }
    }
}
