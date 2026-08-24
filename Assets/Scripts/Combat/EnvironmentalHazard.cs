using System.Collections.Generic;
using UnityEngine;
using Roguelite.Core;

namespace Roguelite.Combat
{
    /// <summary>
    /// Component quản lý bẫy môi trường gây sát thương (chông gai, lưỡi cưa, dung nham,...).
    /// Tương tác trực tiếp với giao diện IDamageable và xử lý Cooldown sát thương tránh chết tức thì.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnvironmentalHazard : MonoBehaviour
    {
        [Header("===== Cấu Hình Sát Thương =====")]
        [Tooltip("Lượng sát thương bẫy gây ra mỗi lần kích hoạt.")]
        [SerializeField] private float damage = 15f;

        [Tooltip("Khoảng thời gian chờ (giây) giữa các lần gây sát thương liên tục khi đứng trên bẫy.")]
        [SerializeField] private float damageCooldown = 1.0f;

        [Tooltip("Lực đẩy/nảy áp dụng cho mục tiêu khi chạm bẫy (Ví dụ: Y > 0 để hất nảy nhân vật lên).")]
        [SerializeField] private Vector2 knockbackForce = new Vector2(0f, 6f);

        [Tooltip("Tự động tính hướng lực đẩy nằm ngang (X) dựa theo vị trí nhân vật so với bẫy.")]
        [SerializeField] private bool directionalKnockback = true;

        [Header("===== Mục Tiêu / LayerMask =====")]
        [Tooltip("Các Layer đối tượng sẽ bị gây sát thương (Player, Enemy,...).")]
        [SerializeField] private LayerMask targetLayer;

        [Header("===== Debug Settings =====")]
        [SerializeField] private bool enableDebugLog = true;
        private const string MODULE_NAME = "EnvironmentalHazard";

        // Lưu trữ thời gian gây sát thương gần nhất của từng đối tượng (tránh lặp sát thương 60fps)
        private Dictionary<int, float> lastDamageTimeMap = new Dictionary<int, float>();

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true; // Đảm bảo Collider luôn là Trigger
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryApplyHazardDamage(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryApplyHazardDamage(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            int instanceID = collision.gameObject.GetInstanceID();
            if (lastDamageTimeMap.ContainsKey(instanceID))
            {
                lastDamageTimeMap.Remove(instanceID);
            }
        }

        private void TryApplyHazardDamage(Collider2D collision)
        {
            // Kiểm tra Layer của đối tượng chạm bẫy có nằm trong targetLayer không
            if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;

            // Tìm component IDamageable trên đối tượng va chạm (hoặc GameObject cha)
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = collision.GetComponentInParent<IDamageable>();
            }

            if (damageable == null) return;

            int instanceID = collision.gameObject.GetInstanceID();
            float currentTime = Time.time;

            // Kiểm tra Cooldown sát thương cho từng mục tiêu
            if (lastDamageTimeMap.TryGetValue(instanceID, out float lastTime))
            {
                if (currentTime - lastTime < damageCooldown)
                {
                    return; // Đang trong thời gian hồi chiêu
                }
            }

            // Tính toán hướng Knockback nảy ra ngoài
            Vector2 appliedKnockback = knockbackForce;
            if (directionalKnockback)
            {
                float dirX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                appliedKnockback.x = Mathf.Abs(knockbackForce.x) * (dirX == 0 ? 1 : dirX);
            }

            // Gây sát thương và áp dụng nảy
            damageable.TakeDamage(damage, appliedKnockback);
            lastDamageTimeMap[instanceID] = currentTime;

            if (enableDebugLog)
            {
                DebugLogger.LogWarning($"[EnvironmentalHazard] [{gameObject.name}] Đã gây {damage} sát thương lên {collision.gameObject.name}", MODULE_NAME);
            }
        }
    }
}
