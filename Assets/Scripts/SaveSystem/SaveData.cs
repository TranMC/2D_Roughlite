using System;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Class tổng gộp tất cả dữ liệu tiến trình vĩnh viễn để lưu xuống đĩa.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int saveVersion = 2;
        public int slotIndex = 1;
        public string lastSavedTime = string.Empty;

        public PlayerProgressData progressData = new PlayerProgressData();
        public WeaponUnlockData weaponData = new WeaponUnlockData();
        public AbilityUnlockData abilityData = new AbilityUnlockData();

        public SaveData()
        {
            saveVersion = 2;
            slotIndex = 1;
            lastSavedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            progressData = new PlayerProgressData();
            weaponData = new WeaponUnlockData();
            abilityData = new AbilityUnlockData();
        }
    }
}
