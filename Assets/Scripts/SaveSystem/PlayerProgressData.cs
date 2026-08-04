using System;
using UnityEngine;

namespace Roguelite.SaveSystem
{
    /// <summary>
    /// Lưu trữ tiến trình tổng thể của người chơi qua các lượt chơi (Runs).
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        public int totalCurrency = 0;       // Vàng / Linh hồn tích lũy
        public int totalRunsPlayed = 0;     // Số lượt chơi đã tham gia
        public int totalEnemiesKilled = 0;  // Tổng số quái đã tiêu diệt
        public int highestRoomReached = 0;  // Room sâu nhất đạt được

        public PlayerProgressData()
        {
            totalCurrency = 0;
            totalRunsPlayed = 0;
            totalEnemiesKilled = 0;
            highestRoomReached = 0;
        }
    }
}
