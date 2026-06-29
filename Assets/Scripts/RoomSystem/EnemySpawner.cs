using System;
using System.Collections.Generic;
using UnityEngine;
using Roguelite.Enemy;

namespace Roguelite.RoomSystem
{
    /// <summary>
    /// Lưu trữ thông tin cấu hình sinh quái vật sử dụng tọa độ offset thay vì Transform.
    /// </summary>
    [Serializable]
    public struct EnemySpawnData
    {
        [Tooltip("Prefab của loại quái vật cần spawn.")]
        public GameObject enemyPrefab;

        [Tooltip("Vị trí spawn lệch so với tâm của Spawner/Căn phòng (Local Offset).")]
        public Vector2 spawnOffset;

        [Tooltip("Sử dụng tâm tuần tra tùy chỉnh? Nếu để trống, quái sẽ tự động tuần tra quanh vị trí spawn của nó.")]
        public bool useCustomPatrolCenter;

        [Tooltip("Vị trí tâm tuần tra lệch so với tâm của Spawner/Căn phòng (Chỉ có tác dụng nếu useCustomPatrolCenter = true).")]
        public Vector2 patrolCenterOffset;
    }

    /// <summary>
    /// Component quản lý việc tự động sinh quái vật và theo dõi số lượng quái vật còn sống.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("===== Enemy Spawn Settings =====")]
        [Tooltip("Danh sách các quái vật sẽ được sinh ra.")]
        [SerializeField] private List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();

        // Danh sách các quái vật đã được sinh ra và còn sống
        private List<EnemyBase> spawnedEnemies = new List<EnemyBase>();

        // Sự kiện báo khi toàn bộ quái vật đã bị tiêu diệt sạch
        public event Action OnAllEnemiesCleared;

        /// <summary>
        /// Thực hiện sinh quái vật tại các vị trí định sẵn.
        /// </summary>
        public void SpawnEnemies()
        {
            if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
            {
                Debug.LogWarning($"[EnemySpawner] [{gameObject.name}] Không có cấu hình quái vật nào để sinh!");
                OnAllEnemiesCleared?.Invoke();
                return;
            }

            spawnedEnemies.Clear();
            Debug.Log($"[EnemySpawner] [{gameObject.name}] Bắt đầu sinh {enemiesToSpawn.Count} quái vật.");

            Vector2 parentPos = (Vector2)transform.position;

            foreach (var spawnData in enemiesToSpawn)
            {
                if (spawnData.enemyPrefab == null)
                {
                    Debug.LogWarning($"[EnemySpawner] [{gameObject.name}] Thiếu Prefab trong cấu hình spawn!");
                    continue;
                }

                // Tính toán vị trí spawn thế giới dựa trên spawnOffset
                Vector2 worldSpawnPos = parentPos + spawnData.spawnOffset;

                // Sinh quái vật tại worldSpawnPos
                GameObject enemyObj = Instantiate(spawnData.enemyPrefab, worldSpawnPos, Quaternion.identity);

                // Set parent làm con của Spawner để giữ Hierarchy sạch sẽ
                enemyObj.transform.SetParent(this.transform);

                EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    // Tính toán và gán vị trí tâm tuần tra thế giới
                    Vector2 worldPatrolCenter = spawnData.useCustomPatrolCenter ? (parentPos + spawnData.patrolCenterOffset) : worldSpawnPos;
                    enemy.SetPatrolCenter(worldPatrolCenter);

                    spawnedEnemies.Add(enemy);
                    enemy.OnDied += () => OnEnemyDied(enemy);
                }
                else
                {
                    Debug.LogWarning($"[EnemySpawner] Prefab sinh ra không chứa component kế thừa EnemyBase: {enemyObj.name}");
                }
            }

            // Trường hợp không có quái vật nào được sinh ra thành công
            if (spawnedEnemies.Count == 0)
            {
                OnAllEnemiesCleared?.Invoke();
            }
        }

        private void OnEnemyDied(EnemyBase enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
                Debug.Log($"[EnemySpawner] Quái vật {enemy.gameObject.name} đã chết. Còn lại: {spawnedEnemies.Count}");

                if (spawnedEnemies.Count == 0)
                {
                    Debug.Log("[EnemySpawner] Toàn bộ quái vật đã bị tiêu diệt sạch!");
                    OnAllEnemiesCleared?.Invoke();
                }
            }
        }
    }
}
