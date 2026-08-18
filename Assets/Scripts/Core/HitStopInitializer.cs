using UnityEngine;

namespace Roguelite.Core
{
    /// <summary>
    /// Script tự động khởi tạo HitStopManager trong scene.
    /// Gắn script này vào một GameObject trong scene (ví dụ: GameManager hoặc một GameObject rỗng).
    /// </summary>
    public class HitStopInitializer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Tự động tạo HitStopManager nếu chưa tồn tại")]
        [SerializeField] private bool autoCreateOnAwake = true;

        private void Awake()
        {
            if (autoCreateOnAwake && HitStopManager.Instance == null)
            {
                GameObject hitStopManagerGO = new GameObject("HitStopManager");
                hitStopManagerGO.AddComponent<HitStopManager>();
                Debug.Log("[HitStopInitializer] Đã tạo HitStopManager tự động");
            }
        }
    }
}