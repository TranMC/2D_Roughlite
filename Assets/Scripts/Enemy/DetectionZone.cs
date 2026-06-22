using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Roguelite.Enemy
{
    /// <summary>
    /// Vùng cảm biến trigger giúp phát hiện các đối tượng (Collider2D) đi vào và đi ra khỏi vùng quét.
    /// Thích hợp để làm vùng phát hiện người chơi, tầm đánh của quái vật.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DetectionZone : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool logDetection = false;

        [Header("Detection Events")]
        public UnityEvent noCollidersRemain;
        
        [Header("Detected Targets")]
        [SerializeField] private List<Collider2D> detectedColliders = new List<Collider2D>();
        public List<Collider2D> DetectedColliders => detectedColliders;

        private Collider2D col;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            col.isTrigger = true; // Bắt buộc phải là Trigger
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!detectedColliders.Contains(collision))
            {
                detectedColliders.Add(collision);
                
                if (logDetection)
                {
                    Debug.Log($"[DetectionZone] [{gameObject.name}] Đối tượng đi VÀO: {collision.name} | Tổng số: {detectedColliders.Count}");
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (detectedColliders.Contains(collision))
            {
                detectedColliders.Remove(collision);
                
                if (logDetection)
                {
                    Debug.Log($"[DetectionZone] [{gameObject.name}] Đối tượng đi RA: {collision.name} | Còn lại: {detectedColliders.Count}");
                }

                if (detectedColliders.Count == 0)
                {
                    noCollidersRemain?.Invoke();
                }
            }
        }

        /// <summary>
        /// Xóa sạch danh sách đối tượng đã phát hiện (ví dụ khi bị vô hiệu hóa).
        /// </summary>
        public void ClearDetected()
        {
            detectedColliders.Clear();
        }
    }
}
