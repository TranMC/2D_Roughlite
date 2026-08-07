using UnityEngine;
using UnityEngine.Events;
using Roguelite.Combat;

namespace Roguelite.Core
{
    /// <summary>
    /// Lưu trữ các Event tĩnh toàn cục dùng để thông báo giữa các hệ thống (như UI, Audio) khi có sự kiện nhân vật bị thương, hồi máu hoặc checkpoint.
    /// </summary>
    public static class CharacterEvents
    {
        // Sự kiện xảy ra khi bất kỳ nhân vật nào nhận sát thương (GameObject nhận dmg, Lượng dmg)
        public static UnityAction<GameObject, int> characterDamaged;

        // Sự kiện xảy ra khi bất kỳ nhân vật nào được hồi máu (GameObject được hồi, Lượng hồi)
        public static UnityAction<GameObject, int> characterHealed;

        // Checkpoint event - xảy ra khi người chơi kích hoạt điểm checkpoint (Vector3 vị trí checkpoint)
        public static UnityAction<Vector3> checkpointSaved;

        // Sự kiện xảy ra khi Player đổi vũ khí thành công (GameObject player, WeaponData vũ khí mới)
        public static UnityAction<GameObject, WeaponData> weaponSwitched;
    }
}
