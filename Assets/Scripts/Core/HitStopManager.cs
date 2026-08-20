using System.Collections;
using UnityEngine;

namespace Roguelite.Core
{
    /// <summary>
    /// Quản lý hiệu ứng Hit-stop (frame freeze) với các mức độ khác nhau.
    /// Hit-stop tạm dừng thời gian (timescale = 0) trong một khoảng thời gian ngắn
    /// để tạo cảm giác mạnh cho các đòn đánh quan trọng.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        [Header("Hit Stop Durations")]
        [Tooltip("Thời gian hit-stop nhẹ (giây) - cho player tấn công enemy")]
        [SerializeField] private float lightHitStopDuration = 0.05f;

        [Tooltip("Thời gian hit-stop trung bình (giây) - cho player bị tấn công hoặc enemy thường chết")]
        [SerializeField] private float mediumHitStopDuration = 0.1f;

        [Tooltip("Thời gian hit-stop nặng (giây) - cho boss hoặc player chết")]
        [SerializeField] private float heavyHitStopDuration = 0.2f;

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebug = true;
        [SerializeField] private bool logHitStop = true;

        private Coroutine currentHitStopCoroutine;
        private bool isHitStopActive = false;
        private HitStopIntensity currentIntensity = HitStopIntensity.Custom;

        public float LightHitStopDuration => lightHitStopDuration;
        public float MediumHitStopDuration => mediumHitStopDuration;
        public float HeavyHitStopDuration => heavyHitStopDuration;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Kích hoạt hit-stop nhẹ cho player tấn công enemy
        /// </summary>
        public void LightHitStop()
        {
            TriggerHitStop(lightHitStopDuration, HitStopIntensity.Light);
        }

        /// <summary>
        /// Kích hoạt hit-stop trung bình cho player bị tấn công hoặc enemy thường chết
        /// </summary>
        public void MediumHitStop()
        {
            TriggerHitStop(mediumHitStopDuration, HitStopIntensity.Medium);
        }

        /// <summary>
        /// Kích hoạt hit-stop nặng cho boss hoặc player chết
        /// </summary>
        public void HeavyHitStop()
        {
            TriggerHitStop(heavyHitStopDuration, HitStopIntensity.Heavy);
        }

        /// <summary>
        /// Kích hoạt hit-stop với thời gian tùy chỉnh
        /// </summary>
        /// <param name="duration">Thời gian hit-stop (giây)</param>
        /// <param name="intensity">Mức độ hit-stop (để debug)</param>
        public void TriggerHitStop(float duration, HitStopIntensity intensity = HitStopIntensity.Custom)
        {
            if (duration <= 0f) return;

            // Không cho mức thấp hơn ghi đè hit-stop đang chạy (vd: Light sau Medium/Heavy)
            if (isHitStopActive && intensity < currentIntensity)
            {
                if (logHitStop && enableDebug)
                {
                    Debug.Log($"[HitStopManager] Bỏ qua {intensity} hit-stop (đang chạy {currentIntensity})");
                }
                return;
            }

            if (currentHitStopCoroutine != null)
            {
                StopCoroutine(currentHitStopCoroutine);
            }

            currentHitStopCoroutine = StartCoroutine(HitStopCoroutine(duration, intensity));
        }

        private IEnumerator HitStopCoroutine(float duration, HitStopIntensity intensity)
        {
            isHitStopActive = true;
            currentIntensity = intensity;

            if (logHitStop && enableDebug)
            {
                Debug.Log($"[HitStopManager] {intensity} hit-stop started for {duration}s");
            }

            // Đặt timescale về 0 để tạm dừng thời gian
            Time.timeScale = 0f;

            // Chờ trong thời gian hit-stop (sử dụng unscaledDeltaTime để không bị ảnh hưởng bởi timescale = 0)
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Khôi phục timescale về bình thường
            Time.timeScale = 1f;
            isHitStopActive = false;
            currentIntensity = HitStopIntensity.Custom;
            currentHitStopCoroutine = null;

            if (logHitStop && enableDebug)
            {
                Debug.Log($"[HitStopManager] Hit-stop ended");
            }
        }

        /// <summary>
        /// Kiểm tra xem hit-stop có đang hoạt động không
        /// </summary>
        public bool IsHitStopActive => isHitStopActive;

        /// <summary>
        /// Hủy hit-stop hiện tại và khôi phục timescale
        /// </summary>
        public void CancelHitStop()
        {
            if (currentHitStopCoroutine != null)
            {
                StopCoroutine(currentHitStopCoroutine);
                currentHitStopCoroutine = null;
            }

            if (isHitStopActive)
            {
                Time.timeScale = 1f;
                isHitStopActive = false;
                currentIntensity = HitStopIntensity.Custom;

                if (logHitStop && enableDebug)
                {
                    Debug.Log("[HitStopManager] Hit-stop cancelled");
                }
            }
        }

        // ====== EDITOR HELPERS ======
        
        [ContextMenu("Test/Light Hit Stop")]
        private void TestLightHitStop() => LightHitStop();

        [ContextMenu("Test/Medium Hit Stop")]
        private void TestMediumHitStop() => MediumHitStop();

        [ContextMenu("Test/Heavy Hit Stop")]
        private void TestHeavyHitStop() => HeavyHitStop();

        [ContextMenu("Test/Cancel Hit Stop")]
        private void TestCancelHitStop() => CancelHitStop();
    }

    /// <summary>
    /// Các mức độ hit-stop có sẵn
    /// </summary>
    public enum HitStopIntensity
    {
        Light,
        Medium,
        Heavy,
        Custom
    }
}