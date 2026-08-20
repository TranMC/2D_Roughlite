using UnityEngine;

namespace Roguelite.BuffSystem
{
    /// <summary>
    /// Cấu hình dữ liệu cho một loại buff (icon, particle zone, loại buff).
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuffDefinition", menuName = "Roguelite/Buff System/Buff Definition")]
    public class BuffDefinition : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private BuffType buffType;
        [SerializeField] private string displayName;

        [Header("Timing")]
        [Tooltip("Thời gian buff có hiệu lực (giây).")]
        [SerializeField] private float duration = 10f;

        [Header("Visual")]
        [Tooltip("Icon hiển thị phía trên đầu player khi có buff.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Particle prefab dùng để đánh dấu buff zone trên mặt đất.")]
        [SerializeField] private GameObject zoneParticlePrefab;

        public BuffType BuffType => buffType;
        public string DisplayName => displayName;
        public float Duration => duration;
        public Sprite Icon => icon;
        public GameObject ZoneParticlePrefab => zoneParticlePrefab;
    }
}
