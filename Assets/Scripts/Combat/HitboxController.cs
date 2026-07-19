using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Combat
{
    public class HitboxController : MonoBehaviour
    {
        [Header("Hitbox Data Sets")]
        [Tooltip("Mỗi entry map một tên animation state (trong Animator Controller) tới HitboxData.\n" +
                 "Khi ActivateHitbox được gọi, controller kiểm tra state hiện tại của Animator " +
                 "và dùng HitboxData tương ứng để tra cứu frame index.")]
        [SerializeField] private HitboxDataSet[] hitboxDataSets = System.Array.Empty<HitboxDataSet>();

        [Header("References")]
        [SerializeField] private Animator animator;

        private Dictionary<string, HitboxData> dataLookup;
        private Dictionary<string, GameObject> childLookup;
        private Coroutine autoDeactivateCoroutine;

        [System.Serializable]
        public struct HitboxDataSet
        {
            public string animationStateName;
            public HitboxData hitboxData;
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();

            BuildDataLookup();
            BuildChildLookup();

            DeactivateHitboxes();
        }

        private void BuildDataLookup()
        {
            dataLookup = new Dictionary<string, HitboxData>();
            foreach (var set in hitboxDataSets)
            {
                if (!string.IsNullOrEmpty(set.animationStateName) && set.hitboxData != null)
                {
                    dataLookup[set.animationStateName] = set.hitboxData;
                }
            }
        }

        private void BuildChildLookup()
        {
            childLookup = new Dictionary<string, GameObject>();
            foreach (Transform child in transform)
            {
                if (!childLookup.ContainsKey(child.name))
                {
                    childLookup[child.name] = child.gameObject;
                }
            }
        }

        /// <summary>
        /// Tìm HitboxData phù hợp với animation state hiện tại của Animator.
        /// </summary>
        private HitboxData GetCurrentHitboxData()
        {
            if (animator == null || dataLookup == null) return null;

            for (int layer = 0; layer < animator.layerCount; layer++)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);

                foreach (var kvp in dataLookup)
                {
                    if (stateInfo.IsName(kvp.Key))
                        return kvp.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Được gọi từ Animation Event. Kích hoạt hitbox tương ứng với frame index.
        /// </summary>
        public void ActivateHitbox(int frameIndex)
        {
            var data = GetCurrentHitboxData();
            if (data == null)
            {
                Debug.LogWarning($"[HitboxController] {gameObject.name}: Không tìm thấy HitboxData cho state hiện tại.");
                return;
            }

            var entry = data.GetEntryForFrame(frameIndex);
            if (entry == null)
            {
                Debug.LogWarning($"[HitboxController] {gameObject.name}: Không tìm thấy entry cho frame index {frameIndex}.");
                return;
            }

            var activeEntry = entry.Value;

            // Tắt toàn bộ hitbox children trước
            foreach (var kvp in childLookup)
            {
                if (kvp.Value.activeSelf)
                    kvp.Value.SetActive(false);
            }

            // Kích hoạt child mục tiêu
            if (!string.IsNullOrEmpty(activeEntry.hitboxChildName) &&
                childLookup.TryGetValue(activeEntry.hitboxChildName, out var targetChild))
            {
                targetChild.SetActive(true);

                // Đảm bảo Collider2D được bật
                var col = targetChild.GetComponent<Collider2D>();
                if (col != null && !col.enabled)
                    col.enabled = true;

                // Apply damage/knockback override nếu có
                if (activeEntry.damageOverride > 0f || activeEntry.knockbackOverride != Vector2.zero)
                {
                    var attack = targetChild.GetComponent<Attack>();
                    if (attack != null)
                    {
                        if (activeEntry.damageOverride > 0f)
                            attack.AttackDamage = activeEntry.damageOverride;
                        if (activeEntry.knockbackOverride != Vector2.zero)
                            attack.Knockback = activeEntry.knockbackOverride;
                    }
                }

                Debug.Log($"[HitboxController] {gameObject.name}: Kích hoạt '{activeEntry.hitboxChildName}' (frame {frameIndex})");
            }
            else
            {
                Debug.LogWarning($"[HitboxController] {gameObject.name}: Không tìm thấy child '{activeEntry.hitboxChildName}'.");
            }

            // Auto-deactivate
            if (activeEntry.autoDeactivateTime > 0f)
            {
                if (autoDeactivateCoroutine != null) StopCoroutine(autoDeactivateCoroutine);
                autoDeactivateCoroutine = StartCoroutine(AutoDeactivateRoutine(activeEntry.autoDeactivateTime));
            }
        }

        /// <summary>
        /// Được gọi từ Animation Event. Tắt toàn bộ hitbox.
        /// </summary>
        public void DeactivateHitboxes()
        {
            if (autoDeactivateCoroutine != null)
            {
                StopCoroutine(autoDeactivateCoroutine);
                autoDeactivateCoroutine = null;
            }

            foreach (var kvp in childLookup)
            {
                if (kvp.Value.activeSelf)
                    kvp.Value.SetActive(false);
                // When reactivated, ActivateHitbox will re-enable the collider
            }
        }

        public void SetHitboxDataForState(string stateName, HitboxData data)
        {
            if (dataLookup == null)
                dataLookup = new Dictionary<string, HitboxData>();

            dataLookup[stateName] = data;

            // Update serialized array for persistence
            for (int i = 0; i < hitboxDataSets.Length; i++)
            {
                if (hitboxDataSets[i].animationStateName == stateName)
                {
                    hitboxDataSets[i].hitboxData = data;
                    return;
                }
            }

            var newList = new List<HitboxDataSet>(hitboxDataSets)
            {
                new HitboxDataSet { animationStateName = stateName, hitboxData = data }
            };
            hitboxDataSets = newList.ToArray();
        }

        private IEnumerator AutoDeactivateRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            DeactivateHitboxes();
        }
    }
}
