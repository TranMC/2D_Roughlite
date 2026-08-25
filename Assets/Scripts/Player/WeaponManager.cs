using UnityEngine;
using Roguelite.Combat;
using Roguelite.Core;
using Roguelite.SaveSystem;

namespace Roguelite.Player
{
    /// <summary>
    /// Quản lý việc áp dụng các chỉ số Support Buffs từ tối đa 3 vũ khí trang bị trong lượt run hiện tại.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        private void OnEnable()
        {
            WeaponShopManager.OnWeaponEquipped += HandleWeaponEquippedFromShop;
            WeaponShopManager.OnWeaponUnlockStateChanged += ApplyCurrentSupportBuffs;
        }

        private void OnDisable()
        {
            WeaponShopManager.OnWeaponEquipped -= HandleWeaponEquippedFromShop;
            WeaponShopManager.OnWeaponUnlockStateChanged -= ApplyCurrentSupportBuffs;
        }

        private void Start()
        {
            ApplyCurrentSupportBuffs();
        }

        private void HandleWeaponEquippedFromShop(WeaponData weapon)
        {
            ApplyCurrentSupportBuffs();
        }

        /// <summary>
        /// Áp dụng các chỉ số cộng dồn (Support Buffs) từ các vũ khí trang bị lên Player.
        /// </summary>
        public void ApplyCurrentSupportBuffs()
        {
            if (WeaponShopManager.Instance != null)
            {
                WeaponShopManager.Instance.ApplyEquippedWeaponBuffs(gameObject);
            }
        }
    }
}
