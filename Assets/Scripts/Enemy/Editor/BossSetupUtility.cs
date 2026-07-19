#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Roguelite.Enemy;
using Roguelite.Combat;

namespace Roguelite.Editor
{
    [InitializeOnLoad]
    public static class BossSetupUtility
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Enemy/Boss.prefab";
        private const string PATTERNS_DIR = "Assets/Prefabs/Enemy/AttackPatterns";

        static BossSetupUtility()
        {
            // Tự động chạy thiết lập đúng 1 lần khi code được import/compile trong Unity Editor
            if (!EditorPrefs.GetBool("Roguelite_BossSetupCompleted", false))
            {
                EditorApplication.delayCall += () =>
                {
                    SetupBoss();
                    EditorPrefs.SetBool("Roguelite_BossSetupCompleted", true);
                };
            }
        }

        [MenuItem("Tools/Roguelite/Setup Boss Attack Patterns")]
        public static void SetupBoss()
        {
            Debug.Log("[BossSetupUtility] Bắt đầu tự động thiết lập Boss và Attack Patterns...");

            // 1. Tạo thư mục chứa các ScriptableObject nếu chưa có
            if (!Directory.Exists(PATTERNS_DIR))
            {
                Directory.CreateDirectory(PATTERNS_DIR);
                AssetDatabase.Refresh();
            }

            // 2. Tạo hoặc tải các AttackPattern assets
            AttackPattern quickJab = CreateOrGetPattern("QuickJabPattern", "AI_attack", 0.5f, 0.1f, 0.15f, true, new Vector2(1f, 0f), new Vector2(1f, 0f), 10f, new Vector2(3f, 1f));
            AttackPattern heavySwing = CreateOrGetPattern("HeavySwingPattern", "AI_attack", 1.2f, 0.6f, 0.3f, false, new Vector2(1.5f, 0f), new Vector2(2.5f, 1.5f), 25f, new Vector2(8f, 3f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3. Tải Prefab Boss dưới dạng GameObject để chỉnh sửa
            GameObject bossRoot = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (bossRoot == null)
            {
                Debug.LogError($"[BossSetupUtility] Không tìm thấy prefab Boss tại đường dẫn: {PREFAB_PATH}");
                return;
            }

            try
            {
                // Tìm hoặc thêm component EntityHitboxHandler
                EntityHitboxHandler hitboxHandler = bossRoot.GetComponentInChildren<EntityHitboxHandler>();
                if (hitboxHandler == null)
                {
                    // Thử tìm object con chuyên trách hitbox hoặc thêm thẳng vào root
                    Transform hitboxTransform = bossRoot.transform.Find("Hitbox");
                    GameObject targetObj = hitboxTransform != null ? hitboxTransform.gameObject : bossRoot;
                    
                    hitboxHandler = targetObj.AddComponent<EntityHitboxHandler>();
                    Debug.Log($"[BossSetupUtility] Đã tự động thêm EntityHitboxHandler vào {targetObj.name}.");
                }

                // Cấu hình các trường trong component Boss
                Boss bossComponent = bossRoot.GetComponent<Boss>();
                if (bossComponent != null)
                {
                    // Sử dụng SerializedObject để gán các private serialized fields
                    SerializedObject serializedBoss = new SerializedObject(bossComponent);
                    
                    // Gán hitboxHandler
                    SerializedProperty handlerProp = serializedBoss.FindProperty("hitboxHandler");
                    handlerProp.objectReferenceValue = hitboxHandler;

                    // Cấu hình phasePatterns list
                    SerializedProperty phasePatternsProp = serializedBoss.FindProperty("phasePatterns");
                    phasePatternsProp.ClearArray();

                    // Phase 0 (Default) -> QuickJab
                    phasePatternsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty phase0 = phasePatternsProp.GetArrayElementAtIndex(0);
                    phase0.FindPropertyRelative("phaseName").stringValue = "Phase 0 (Default)";
                    SerializedProperty phase0Patterns = phase0.FindPropertyRelative("patterns");
                    phase0Patterns.ClearArray();
                    phase0Patterns.InsertArrayElementAtIndex(0);
                    phase0Patterns.GetArrayElementAtIndex(0).objectReferenceValue = quickJab;

                    // Phase 1 (Yellow) -> QuickJab, HeavySwing
                    phasePatternsProp.InsertArrayElementAtIndex(1);
                    SerializedProperty phase1 = phasePatternsProp.GetArrayElementAtIndex(1);
                    phase1.FindPropertyRelative("phaseName").stringValue = "Phase 1 (Yellow)";
                    SerializedProperty phase1Patterns = phase1.FindPropertyRelative("patterns");
                    phase1Patterns.ClearArray();
                    phase1Patterns.InsertArrayElementAtIndex(0);
                    phase1Patterns.GetArrayElementAtIndex(0).objectReferenceValue = quickJab;
                    phase1Patterns.InsertArrayElementAtIndex(1);
                    phase1Patterns.GetArrayElementAtIndex(1).objectReferenceValue = heavySwing;

                    // Phase 2 (Red) -> HeavySwing
                    phasePatternsProp.InsertArrayElementAtIndex(2);
                    SerializedProperty phase2 = phasePatternsProp.GetArrayElementAtIndex(2);
                    phase2.FindPropertyRelative("phaseName").stringValue = "Phase 2 (Red)";
                    SerializedProperty phase2Patterns = phase2.FindPropertyRelative("patterns");
                    phase2Patterns.ClearArray();
                    phase2Patterns.InsertArrayElementAtIndex(0);
                    phase2Patterns.GetArrayElementAtIndex(0).objectReferenceValue = heavySwing;

                    serializedBoss.ApplyModifiedProperties();
                    Debug.Log("[BossSetupUtility] Đã tự động cấu hình các Phase Attack Patterns cho Boss component.");
                }
                else
                {
                    Debug.LogError("[BossSetupUtility] Không tìm thấy component Boss trên prefab root!");
                }

                // Lưu các thay đổi vào prefab
                PrefabUtility.SaveAsPrefabAsset(bossRoot, PREFAB_PATH);
                Debug.Log($"[BossSetupUtility] Đã lưu các thay đổi vào prefab thành công: {PREFAB_PATH}");
            }
            finally
            {
                // Giải phóng bộ nhớ prefab contents
                PrefabUtility.UnloadPrefabContents(bossRoot);
            }
        }

        private static AttackPattern CreateOrGetPattern(
            string name, 
            string animTrigger, 
            float lockDuration, 
            float startDelay, 
            float activeDuration, 
            bool isCircle, 
            Vector2 offset, 
            Vector2 size, 
            float damage, 
            Vector2 knockback)
        {
            string path = $"{PATTERNS_DIR}/{name}.asset";
            AttackPattern pattern = AssetDatabase.LoadAssetAtPath<AttackPattern>(path);

            if (pattern == null)
            {
                pattern = ScriptableObject.CreateInstance<AttackPattern>();
                AssetDatabase.CreateAsset(pattern, path);
                Debug.Log($"[BossSetupUtility] Đã tạo mới AttackPattern asset: {path}");
            }

            // Sử dụng SerializedObject để cấu hình giá trị riêng tư của ScriptableObject
            SerializedObject serializedPattern = new SerializedObject(pattern);
            serializedPattern.FindProperty("patternName").stringValue = name;
            serializedPattern.FindProperty("animationTrigger").stringValue = animTrigger;
            serializedPattern.FindProperty("attackLockDuration").floatValue = lockDuration;
            serializedPattern.FindProperty("useAnimationEvents").boolValue = false;
            serializedPattern.FindProperty("startDelay").floatValue = startDelay;
            serializedPattern.FindProperty("activeDuration").floatValue = activeDuration;
            serializedPattern.FindProperty("isCircle").boolValue = isCircle;
            serializedPattern.FindProperty("hitboxOffset").vector2Value = offset;
            serializedPattern.FindProperty("hitboxSize").vector2Value = size;
            serializedPattern.FindProperty("damage").floatValue = damage;
            serializedPattern.FindProperty("knockback").vector2Value = knockback;

            serializedPattern.ApplyModifiedProperties();
            EditorUtility.SetDirty(pattern);

            return pattern;
        }
    }
}
#endif
