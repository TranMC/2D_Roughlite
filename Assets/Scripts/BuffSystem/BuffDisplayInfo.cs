namespace Roguelite.BuffSystem
{
    /// <summary>
    /// Dữ liệu hiển thị một icon buff, bao gồm thời gian còn lại để xử lý nhấp nháy cảnh báo.
    /// </summary>
    public readonly struct BuffDisplayInfo
    {
        public BuffDefinition Definition { get; }
        public float RemainingTime { get; }
        public float TotalDuration { get; }

        public BuffDisplayInfo(BuffDefinition definition, float remainingTime, float totalDuration)
        {
            Definition = definition;
            RemainingTime = remainingTime;
            TotalDuration = totalDuration;
        }
    }
}
