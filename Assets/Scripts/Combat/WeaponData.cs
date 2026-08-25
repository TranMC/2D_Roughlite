using UnityEngine;

namespace Roguelite.Combat
{
    /// <summary>
    /// ScriptableObject lưu trữ thông số cơ bản của một vũ khí.
    /// Được truyền qua event weaponSwitched khi Player đổi vũ khí.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Roguelite/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [SerializeField] private string weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private Sprite icon;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Chỉ số chiến đấu")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float range = 1.5f;
        [SerializeField] private Vector2 knockback = new Vector2(3f, 0f);

        [Header("Cửa Hàng & Điều Kiện Mở Khóa")]
        [Tooltip("Giá mua vũ khí trong Cửa hàng (Gold).")]
        [SerializeField] private int price = 100;

        [Tooltip("Số lượng quái tiêu diệt tối thiểu để đủ điều kiện mở khóa.")]
        [SerializeField] private int requiredEnemiesKilled = 0;

        [Tooltip("Số lượt chơi (Runs) tối thiểu để đủ điều kiện mở khóa.")]
        [SerializeField] private int requiredRunsPlayed = 0;

        [Tooltip("Cấp phòng sâu nhất (Highest Room) tối thiểu để đủ điều kiện mở khóa.")]
        [SerializeField] private int requiredHighestRoom = 0;

        [Tooltip("Đã tự động mở khóa mặc định ngay từ đầu game (Vd: Starter Sword).")]
        [SerializeField] private bool isDefaultUnlocked = false;

        // === Properties (Getters & Setters hỗ trợ Runtime Editing & Editor Tools) ===
        public string WeaponId { get => weaponId; set => weaponId = value; }
        public string WeaponName { get => weaponName; set => weaponName = value; }
        public Sprite Icon => icon;
        public string Description => description;
        public float Damage { get => damage; set => damage = value; }
        public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
        public float Range { get => range; set => range = value; }
        public Vector2 Knockback { get => knockback; set => knockback = value; }

        public int Price { get => price; set => price = value; }
        public int RequiredEnemiesKilled { get => requiredEnemiesKilled; set => requiredEnemiesKilled = value; }
        public int RequiredRunsPlayed { get => requiredRunsPlayed; set => requiredRunsPlayed = value; }
        public int RequiredHighestRoom { get => requiredHighestRoom; set => requiredHighestRoom = value; }
        public bool IsDefaultUnlocked { get => isDefaultUnlocked; set => isDefaultUnlocked = value; }

        /// <summary>
        /// Kiểm tra xem người chơi đã đạt đủ tất cả các điều kiện mở khóa để cho phép mua hay chưa.
        /// </summary>
        public bool IsRequirementMet(Roguelite.SaveSystem.PlayerProgressData progress)
        {
            if (isDefaultUnlocked) return true;
            if (progress == null) return false;

            return progress.totalEnemiesKilled >= requiredEnemiesKilled
                && progress.totalRunsPlayed >= requiredRunsPlayed
                && progress.highestRoomReached >= requiredHighestRoom;
        }
    }
}