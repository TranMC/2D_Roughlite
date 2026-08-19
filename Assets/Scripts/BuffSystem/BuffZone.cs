using UnityEngine;

namespace Roguelite.BuffSystem
{
    /// <summary>
    /// Vùng buff trên mặt đất. Khi player chạm vào sẽ biến mất và cấp buff (visual) cho player.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BuffZone : MonoBehaviour
    {
        [Header("Buff Settings")]
        [SerializeField] private BuffDefinition buffDefinition;

        [Header("Visual")]
        [Tooltip("Transform chứa particle visual. Nếu để trống sẽ tự spawn từ BuffDefinition.")]
        [SerializeField] private Transform visualRoot;

        [Header("Pickup")]
        [SerializeField] private bool destroyOnPickup = true;

        private bool collected;

        private void Awake()
        {
            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
        }

        private void Start()
        {
            SpawnVisualIfNeeded();
        }

        private void SpawnVisualIfNeeded()
        {
            if (buffDefinition == null || buffDefinition.ZoneParticlePrefab == null)
            {
                return;
            }

            if (visualRoot != null && visualRoot.childCount > 0)
            {
                return;
            }

            Transform parent = visualRoot != null ? visualRoot : transform;
            Instantiate(buffDefinition.ZoneParticlePrefab, parent);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected || buffDefinition == null)
            {
                return;
            }

            PlayerBuffManager buffManager = collision.GetComponent<PlayerBuffManager>();
            if (buffManager == null)
            {
                buffManager = collision.GetComponentInParent<PlayerBuffManager>();
            }

            if (buffManager == null)
            {
                return;
            }

            collected = true;
            buffManager.ApplyBuff(buffDefinition);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
