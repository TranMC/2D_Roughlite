using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Combat
{
    /// <summary>
    /// ScriptableObject tập hợp quản lý danh sách toàn bộ WeaponData trong game.
    /// Cho phép truy vấn tất cả vũ khí và tìm kiếm theo WeaponId.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Roguelite/Combat/Weapon Database")]
    public class WeaponDatabase : ScriptableObject
    {
        [Header("Danh Sách Vũ Khí Trong Game")]
        [SerializeField] private List<WeaponData> allWeapons = new List<WeaponData>();

        public List<WeaponData> AllWeapons => allWeapons;

        /// <summary>
        /// Lấy thông tin WeaponData theo WeaponId.
        /// </summary>
        public WeaponData GetWeaponById(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId) || allWeapons == null) return null;

            foreach (var weapon in allWeapons)
            {
                if (weapon == null) continue;
                string id = string.IsNullOrEmpty(weapon.WeaponId) ? weapon.name : weapon.WeaponId;
                if (id == weaponId)
                {
                    return weapon;
                }
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra xem WeaponId có tồn tại trong database hay không.
        /// </summary>
        public bool ContainsWeapon(string weaponId)
        {
            return GetWeaponById(weaponId) != null;
        }
    }
}
