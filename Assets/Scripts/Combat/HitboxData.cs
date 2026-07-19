using UnityEngine;

namespace Roguelite.Combat
{
    [CreateAssetMenu(fileName = "NewHitboxData", menuName = "Roguelite/Combat/Hitbox Data")]
    public class HitboxData : ScriptableObject
    {
        [System.Serializable]
        public struct HitboxFrameEntry
        {
            [Tooltip("Frame index trong animation clip (0-based). Animation event ActivateHitbox(int) sẽ truyền index này.")]
            public int frameIndex;

            [Tooltip("Tên child GameObject chứa Collider2D và Attack component cần kích hoạt.")]
            public string hitboxChildName;

            [Tooltip("Override sát thương (0 = dùng giá trị mặc định từ Attack component).")]
            public float damageOverride;

            [Tooltip("Override knockback.")]
            public Vector2 knockbackOverride;

            [Tooltip("Tự động tắt hitbox sau N giây (0 = phải gọi DeactivateHitboxes thủ công từ animation event).")]
            public float autoDeactivateTime;
        }

        [SerializeField] private HitboxFrameEntry[] frameEntries = System.Array.Empty<HitboxFrameEntry>();

        public HitboxFrameEntry[] FrameEntries => frameEntries;

#if UNITY_EDITOR
        public void SetFrameEntries(HitboxFrameEntry[] entries)
        {
            frameEntries = entries;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public string[] GetUniqueChildNames()
        {
            var names = new System.Collections.Generic.HashSet<string>();
            foreach (var entry in frameEntries)
            {
                if (!string.IsNullOrEmpty(entry.hitboxChildName))
                    names.Add(entry.hitboxChildName);
            }
            var result = new string[names.Count];
            names.CopyTo(result);
            return result;
        }

        public HitboxFrameEntry? GetEntryForFrame(int frameIndex)
        {
            for (int i = 0; i < frameEntries.Length; i++)
            {
                if (frameEntries[i].frameIndex == frameIndex)
                    return frameEntries[i];
            }
            return null;
        }
    }
}
