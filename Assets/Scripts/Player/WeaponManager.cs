using UnityEngine;
using Roguelite.Combat;
using Roguelite.Core;

namespace Roguelite.Player
{
    /// <summary>
    /// Quản lý danh sách vũ khí đã unlock và vũ khí đang trang bị.
    /// Phát event CharacterEvents.weaponSwitched khi đổi vũ khí thành công.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Inventory")]
        [Tooltip("Danh sách vũ khí đã mở khóa (kéo thả WeaponData vào đây).")]
        [SerializeField] private WeaponData[] unlockedWeapons;

        [Header("Vũ khí mặc định khi bắt đầu")]
        [Tooltip("Index trong mảng unlockedWeapons sẽ được trang bị khi Start.")]
        [SerializeField] private int defaultWeaponIndex = 0;

        private int currentWeaponIndex = -1;
        private WeaponData currentWeapon;

        /// <summary>Vũ khí đang được trang bị (read-only).</summary>
        public WeaponData CurrentWeapon => currentWeapon;

        /// <summary>Index hiện tại trong mảng unlockedWeapons.</summary>
        public int CurrentWeaponIndex => currentWeaponIndex;

        private void Start()
        {
            if (unlockedWeapons != null && unlockedWeapons.Length > 0)
            {
                EquipWeapon(defaultWeaponIndex);
            }
            else
            {
                Debug.LogWarning($"[WeaponManager] {gameObject.name}: Chưa gán vũ khí nào vào unlockedWeapons!");
            }
        }

        // =====================================================================
        //  PUBLIC API
        // =====================================================================

        /// <summary>
        /// Trang bị vũ khí theo index trong mảng unlockedWeapons.
        /// Phát event weaponSwitched nếu đổi thành công.
        /// </summary>
        public void EquipWeapon(int index)
        {
            if (unlockedWeapons == null || unlockedWeapons.Length == 0) return;

            index = Mathf.Clamp(index, 0, unlockedWeapons.Length - 1);

            // Không đổi nếu đang cầm đúng vũ khí đó rồi
            if (index == currentWeaponIndex) return;

            currentWeaponIndex = index;
            currentWeapon = unlockedWeapons[currentWeaponIndex];

            // === PHÁT EVENT ===
            CharacterEvents.weaponSwitched?.Invoke(gameObject, currentWeapon);

            Debug.Log($"[WeaponManager] {gameObject.name} đổi sang vũ khí: {currentWeapon.WeaponName}");
        }

        /// <summary>
        /// Đổi sang vũ khí tiếp theo trong danh sách (vòng tròn).
        /// </summary>
        public void SwitchToNext()
        {
            if (unlockedWeapons == null || unlockedWeapons.Length <= 1) return;

            int nextIndex = (currentWeaponIndex + 1) % unlockedWeapons.Length;
            EquipWeapon(nextIndex);
        }

        /// Gọi bằng phím Q 
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) SwitchToNext();
        }

        /// <summary>
        /// Đổi sang vũ khí trước đó trong danh sách (vòng tròn).
        /// </summary>
        public void SwitchToPrevious()
        {
            if (unlockedWeapons == null || unlockedWeapons.Length <= 1) return;

            int prevIndex = (currentWeaponIndex - 1 + unlockedWeapons.Length) % unlockedWeapons.Length;
            EquipWeapon(prevIndex);
        }

        /// <summary>
        /// Trang bị vũ khí theo WeaponData (tìm trong danh sách đã unlock).
        /// </summary>
        public void EquipWeapon(WeaponData weaponData)
        {
            if (weaponData == null || unlockedWeapons == null) return;

            for (int i = 0; i < unlockedWeapons.Length; i++)
            {
                if (unlockedWeapons[i] == weaponData)
                {
                    EquipWeapon(i);
                    return;
                }
            }

            Debug.LogWarning($"[WeaponManager] Vũ khí '{weaponData.WeaponName}' chưa được unlock!");
        }
    }
}
