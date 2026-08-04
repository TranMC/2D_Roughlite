using System;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Dữ liệu cài đặt độc lập của người chơi (Âm thanh, Đồ họa, Hệ thống).
    /// </summary>
    [Serializable]
    public class SettingData
    {
        public int settingVersion = 1;

        // Âm thanh
        public float masterVolume = 1f;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 1f;

        // Đồ họa / Hiển thị
        public int screenWidth = 1920;
        public int screenHeight = 1080;
        public bool isFullscreen = true;
        public int targetFrameRate = 60;

        // Hệ thống Slot
        public int lastActiveSlotIndex = 1;

        public SettingData()
        {
            settingVersion = 1;
            lastActiveSlotIndex = 1;
            masterVolume = 1f;
            bgmVolume = 0.8f;
            sfxVolume = 1f;
            screenWidth = 1920;
            screenHeight = 1080;
            isFullscreen = true;
            targetFrameRate = 60;
        }
    }
}
