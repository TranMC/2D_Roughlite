using System;
using System.Collections.Generic;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Dữ liệu các vũ khí đã mở khóa và vũ khí đang được trang bị.
    /// </summary>
    [Serializable]
    public class WeaponUnlockData
    {
        public List<string> unlockedWeaponIds = new List<string>();
        public string equippedWeaponId = string.Empty;

        public WeaponUnlockData()
        {
            unlockedWeaponIds = new List<string>() { "sword_starter" };
            equippedWeaponId = "sword_starter";
        }
    }
}
