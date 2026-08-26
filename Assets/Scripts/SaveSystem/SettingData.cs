using System;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Dữ liệu cài đặt độc lập của người chơi (Âm thanh, Đồ họa, Hệ thống).
    /// </summary>
    [Serializable]
    public class SettingData
    {
        public int settingVersion = 2;

        // Âm thanh
        public float masterVolume = 1f;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 1f;

        // Đồ họa / Hiển thị
        public int screenWidth = 1920;
        public int screenHeight = 1080;
        public bool isFullscreen = true;
        public int targetFrameRate = 144; // Hỗ trợ màn hình 144Hz hoặc không giới hạn (-1)
        public bool enableVSync = false;  // Tắt VSync mặc định để targetFrameRate có hiệu lực tức thì

        // Hệ thống Slot
        public int lastActiveSlotIndex = 1;

        public SettingData()
        {
            settingVersion = 2;
            lastActiveSlotIndex = 1;
            masterVolume = 1f;
            bgmVolume = 0.8f;
            sfxVolume = 1f;
            screenWidth = 1920;
            screenHeight = 1080;
            isFullscreen = true;
            targetFrameRate = 144;
            enableVSync = false;
        }
    }
}
