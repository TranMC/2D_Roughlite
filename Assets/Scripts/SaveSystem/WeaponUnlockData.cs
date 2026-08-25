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
        public const int MAX_EQUIPPED_SLOTS = 3;

        public List<string> unlockedWeaponIds = new List<string>();
        public List<string> equippedWeaponIds = new List<string>();

        public WeaponUnlockData()
        {
            unlockedWeaponIds = new List<string>() { "sword_starter" };
            equippedWeaponIds = new List<string>();
        }
    }
}
