using UnityEngine;
using Roguelite.Core;

namespace Roguelite.Combat
{
    /// <summary>
    /// Cầu nối giữa WeaponManager và HitboxController.
    /// Lắng nghe event weaponSwitched và tự động swap HitboxData + chỉ số sát thương
    /// trên HitboxController và Attack component cho phù hợp với vũ khí mới.
    /// 
    /// Gắn script này lên Player GameObject (cùng chỗ với WeaponManager).
    /// </summary>
    [RequireComponent(typeof(HitboxController))]
    public class WeaponHitboxBridge : MonoBehaviour
    {
        private HitboxController hitboxController;
        private Attack[] attackComponents;

        // Lưu bộ HitboxData gốc (từ Inspector) để khôi phục khi vũ khí không có mapping riêng
        private HitboxController.HitboxDataSet[] defaultDataSets;

        private void Awake()
        {
            hitboxController = GetComponent<HitboxController>();
            attackComponents = GetComponentsInChildren<Attack>(includeInactive: true);
        }

        private void Start()
        {
            // Cache bản gốc trước khi bất kỳ weapon nào swap
            if (hitboxController != null)
            {
                defaultDataSets = hitboxController.GetSerializedDataSets();
            }
        }

        private void OnEnable()
        {
            CharacterEvents.weaponSwitched += OnWeaponSwitched;
        }

        private void OnDisable()
        {
            CharacterEvents.weaponSwitched -= OnWeaponSwitched;
        }

        /// <summary>
        /// Callback khi Player đổi vũ khí thành công.
        /// </summary>
        private void OnWeaponSwitched(GameObject player, WeaponData newWeapon)
        {
            // Chỉ xử lý event phát từ chính Player này
            if (player != gameObject) return;
            if (newWeapon == null) return;

            // 1. Swap HitboxData
            SwapHitboxData(newWeapon);

            // 2. Cập nhật chỉ số sát thương & knockback trên tất cả Attack component
            UpdateAttackStats(newWeapon);

            Debug.Log($"[WeaponHitboxBridge] Đã swap hitbox cho vũ khí: {newWeapon.WeaponName}");
        }

        /// <summary>
        /// Swap bộ HitboxData trên HitboxController theo WeaponData.
        /// Nếu vũ khí không có mapping riêng → khôi phục về bộ mặc định.
        /// </summary>
        private void SwapHitboxData(WeaponData weapon)
        {
            if (hitboxController == null) return;

            if (weapon.HasHitboxMappings)
            {
                // Vũ khí có bộ hitbox riêng → swap từng cặp
                foreach (var mapping in weapon.HitboxMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.animationStateName) && mapping.hitboxData != null)
                    {
                        hitboxController.SetHitboxDataForState(mapping.animationStateName, mapping.hitboxData);
                    }
                }
            }
            else
            {
                // Vũ khí không có hitbox mapping → khôi phục về mặc định
                RestoreDefaultHitboxData();
            }
        }

        /// <summary>
        /// Khôi phục toàn bộ HitboxData về bản gốc (lúc Awake).
        /// </summary>
        private void RestoreDefaultHitboxData()
        {
            if (hitboxController == null || defaultDataSets == null) return;

            foreach (var dataSet in defaultDataSets)
            {
                if (!string.IsNullOrEmpty(dataSet.animationStateName) && dataSet.hitboxData != null)
                {
                    hitboxController.SetHitboxDataForState(dataSet.animationStateName, dataSet.hitboxData);
                }
            }
        }

        /// <summary>
        /// Cập nhật damage và knockback trên tất cả Attack component con
        /// theo chỉ số của vũ khí mới.
        /// </summary>
        private void UpdateAttackStats(WeaponData weapon)
        {
            // Refresh danh sách Attack component (phòng trường hợp có thay đổi runtime)
            if (attackComponents == null || attackComponents.Length == 0)
            {
                attackComponents = GetComponentsInChildren<Attack>(includeInactive: true);
            }

            foreach (var attack in attackComponents)
            {
                if (attack == null) continue;

                attack.AttackDamage = weapon.Damage;
                attack.Knockback = weapon.Knockback;
            }
        }
    }
}
