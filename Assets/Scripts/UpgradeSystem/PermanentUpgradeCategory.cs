namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Enum phân loại các nhóm Nâng cấp vĩnh viễn trong Cửa hàng / UI.
    /// </summary>
    public enum PermanentUpgradeCategory
    {
        All = 0,
        Offense = 1,  // Tấn công (Damage, Critical, etc.)
        Defense = 2,  // Phòng thủ (HP, Armor, Invincibility, etc.)
        Utility = 3   // Đa dụng (MoveSpeed, Jump, Gold multiplier, etc.)
    }
}
