using UnityEditor;
using UnityEngine;

namespace Roguelite.RoomSystem.Editor
{
    [CustomEditor(typeof(EnemySpawner))]
    public class EnemySpawnerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            EnemySpawner spawner = (EnemySpawner)target;
            if (spawner == null) return;

            // Lấy danh sách enemiesToSpawn từ serializedObject để hỗ trợ Undo/Redo và đánh dấu Scene dơ (dirty)
            SerializedProperty enemiesProp = serializedObject.FindProperty("enemiesToSpawn");
            if (enemiesProp == null) return;

            Vector3 spawnerPos = spawner.transform.position;

            serializedObject.Update();

            bool changed = false;

            for (int i = 0; i < enemiesProp.arraySize; i++)
            {
                SerializedProperty spawnDataProp = enemiesProp.GetArrayElementAtIndex(i);
                SerializedProperty prefabProp = spawnDataProp.FindPropertyRelative("enemyPrefab");
                SerializedProperty offsetProp = spawnDataProp.FindPropertyRelative("spawnOffset");
                SerializedProperty useCustomPatrolProp = spawnDataProp.FindPropertyRelative("useCustomPatrolCenter");
                SerializedProperty patrolOffsetProp = spawnDataProp.FindPropertyRelative("patrolCenterOffset");

                string labelName = prefabProp.objectReferenceValue != null 
                    ? prefabProp.objectReferenceValue.name 
                    : $"Enemy [{i}]";

                // --- 1. Vẽ Handle cho Vị trí Spawn ---
                Vector3 currentSpawnPos = spawnerPos + (Vector3)offsetProp.vector2Value;
                
                // Vẽ nhãn văn bản tên quái vật
                Handles.Label(currentSpawnPos + Vector3.up * 0.4f, $"{labelName} (Spawn)", new GUIStyle() {
                    normal = new GUIStyleState() { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                });

                // Vẽ đĩa màu xanh lá biểu thị điểm spawn
                Handles.color = Color.green;
                Handles.DrawWireDisc(currentSpawnPos, Vector3.forward, 0.3f);

                // Tạo Position Handle để kéo thả vị trí spawn
                EditorGUI.BeginChangeCheck();
                Vector3 newSpawnPos = Handles.PositionHandle(currentSpawnPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    offsetProp.vector2Value = (Vector2)(newSpawnPos - spawnerPos);
                    changed = true;
                }

                // --- 2. Vẽ Handle cho Tâm tuần tra nếu bật Custom Patrol Center ---
                if (useCustomPatrolProp.boolValue)
                {
                    Vector3 currentPatrolPos = spawnerPos + (Vector3)patrolOffsetProp.vector2Value;

                    // Vẽ nhãn văn bản
                    Handles.Label(currentPatrolPos + Vector3.down * 0.4f, $"{labelName} (Patrol Center)", new GUIStyle() {
                        normal = new GUIStyleState() { textColor = Color.cyan },
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    });

                    // Vẽ đĩa màu xanh lam biểu thị tâm tuần tra
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(currentPatrolPos, Vector3.forward, 0.2f);
                    // Vẽ đường nối giữa vị trí Spawn và tâm tuần tra
                    Handles.color = new Color(0f, 0.8f, 1f, 0.5f);
                    Handles.DrawDottedLine(currentSpawnPos, currentPatrolPos, 4f);

                    // Tạo Position Handle để kéo thả tâm tuần tra
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPatrolPos = Handles.PositionHandle(currentPatrolPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        patrolOffsetProp.vector2Value = (Vector2)(newPatrolPos - spawnerPos);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                // Đánh dấu scene bị thay đổi để Unity cho phép Save Scene
                EditorUtility.SetDirty(spawner);
            }
        }
    }
}
