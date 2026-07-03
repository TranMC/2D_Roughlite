namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Các loại phòng trong màn chơi.
    /// </summary>
    public enum RoomType
    {
        Start,
        Combat,
        Reward,
        Boss
    }

    /// <summary>
    /// Hướng di chuyển/kết nối của cửa phòng, tương thích với di chuyển platformer 2D.
    /// </summary>
    public enum DoorDirection
    {
        Left,   // Sang trái
        Right,  // Sang phải
        Up,     // Lối đi lên (ví dụ: leo cầu thang)
        Down    // Lối đi xuống (ví dụ: nhảy hố, đi xuống)
    }
}
