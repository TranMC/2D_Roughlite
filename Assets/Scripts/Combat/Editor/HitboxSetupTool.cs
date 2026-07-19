using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Roguelite.Combat;
using Roguelite.Enemy;

namespace Roguelite.Editor
{
    public class HitboxSetupTool : EditorWindow
    {
        private string hitboxDataFolder = "Assets/HitboxData";
        private Vector2 scrollPos;

        // =====================================================================
        //  ENEMY CONFIG: animation clip → hitbox child + frame data
        //  Frame derived from m_Enabled curve times × 60 FPS sample rate
        // =====================================================================

        [System.Serializable]
        private struct HitboxClipEntry
        {
            public int frame;
            public string childName;
            public float autoDeactivateTime; // 0 = manual via DeactivateHitboxes event
        }

        private struct ClipConfig
        {
            public string clipName;
            public string hitboxDataName;
            public HitboxClipEntry[] entries;
        }

        /// <summary>Build a HitboxFrameEntry[] from clip entries for creating HitboxData assets.</summary>
        private static HitboxData.HitboxFrameEntry[] ToFrameEntries(HitboxClipEntry[] clipEntries)
        {
            var list = new List<HitboxData.HitboxFrameEntry>();
            foreach (var ce in clipEntries)
            {
                list.Add(new HitboxData.HitboxFrameEntry
                {
                    frameIndex = ce.frame,
                    hitboxChildName = ce.childName,
                    damageOverride = 0,
                    knockbackOverride = Vector2.zero,
                    autoDeactivateTime = ce.autoDeactivateTime
                });
            }
            return list.ToArray();
        }

        /// <summary>Build HitboxClipEntry with autoDeactivate from deactivateFrame.</summary>
        private static HitboxClipEntry E(int frame, string child, float autoTime = 0f)
            => new HitboxClipEntry { frame = frame, childName = child, autoDeactivateTime = autoTime };

        private static readonly Dictionary<string, ClipConfig[]> EnemyData = new()
        {
            ["Enemy1"] = new[]
            {
                new ClipConfig { clipName = "enemy1_attack1", hitboxDataName = "Enemy1_Attack1_HitboxData", entries = new[] { E(19, "Enemy1_Attack1") } },
                new ClipConfig { clipName = "enemy1_attack2", hitboxDataName = "Enemy1_Attack2_HitboxData", entries = new[] { E(10, "Enemy1_Attack2") } },
                new ClipConfig { clipName = "enemy1_attack3", hitboxDataName = "Enemy1_Attack3_HitboxData", entries = new[] { E(16, "Enemy1_Attack3") } },
            },
            ["Enemy2"] = new[]
            {
                new ClipConfig { clipName = "enemy2_attack1", hitboxDataName = "Enemy2_Attack1_HitboxData", entries = new[] { E(16, "Enemy2_Attack1") } },
                new ClipConfig { clipName = "enemy2_attack2", hitboxDataName = "Enemy2_Attack2_HitboxData", entries = new[] { E(15, "Enemy2_Attack2") } },
                new ClipConfig { clipName = "enemy2_attack3", hitboxDataName = "Enemy2_Attack3_HitboxData", entries = new[] { E(15, "Enemy2_Attack3") } },
            },
            ["Enemy3"] = new[]
            {
                new ClipConfig { clipName = "enemy3_attack1", hitboxDataName = "Enemy3_Attack1_HitboxData", entries = new[] { E(14, "Enemy3_Attack1") } },
                new ClipConfig { clipName = "enemy3_attack2", hitboxDataName = "Enemy3_Attack2_HitboxData", entries = new[] { E(8, "Enemy3_Attack2") } },
                new ClipConfig { clipName = "enemy3_attack3", hitboxDataName = "Enemy3_Attack3_HitboxData", entries = new[] { E(10, "Enemy3_Attack3") } },
            },
            ["Enemy4"] = new[]
            {
                new ClipConfig { clipName = "enemy4_attack1", hitboxDataName = "Enemy4_Attack1_HitboxData", entries = new[] { E(22, "Enemy4_Attack1") } },
                new ClipConfig { clipName = "enemy4_attack2", hitboxDataName = "Enemy4_Attack2_HitboxData", entries = new[] { E(9, "Enemy4_Attack2") } },
                new ClipConfig { clipName = "enemy4_attack3", hitboxDataName = "Enemy4_Attack3_HitboxData", entries = new[] { E(14, "Enemy4_Attack3") } },
            },
            ["Enemy5"] = new[]
            {
                new ClipConfig { clipName = "enemy5_attack1", hitboxDataName = "Enemy5_Attack1_HitboxData", entries = new[] { E(20, "Enemy5_Attack1") } },
                new ClipConfig { clipName = "enemy5_attack2", hitboxDataName = "Enemy5_Attack2_HitboxData", entries = new[] { E(20, "Enemy5_Attack2") } },
                new ClipConfig { clipName = "enemy5_attack3", hitboxDataName = "Enemy5_Attack3_HitboxData", entries = new[] { E(24, "Enemy5_Attack3") } },
            },
            ["Enemy7"] = new[]
            {
                new ClipConfig { clipName = "enemy7_attack1", hitboxDataName = "Enemy7_Attack1_HitboxData", entries = new[] { E(11, "Enemy7_Attack1") } },
                new ClipConfig { clipName = "enemy7_attack2", hitboxDataName = "Enemy7_Attack2_HitboxData", entries = new[] { E(20, "Enemy7_Attack2") } },
                new ClipConfig { clipName = "enemy7_attack3", hitboxDataName = "Enemy7_Attack3_HitboxData", entries = new[] { E(24, "Enemy7_Attack3") } },
            },
        };

        // Player config: air_attack has TWO hitbox windows (frames 0 and 22)
        private static readonly ClipConfig[] PlayerData = new[]
        {
            new ClipConfig { clipName = "player_attack1", hitboxDataName = "Player_Attack1_HitboxData", entries = new[] { E(15, "Attack1") } },
            new ClipConfig { clipName = "player_attack2", hitboxDataName = "Player_Attack2_HitboxData", entries = new[] { E(13, "Attack2") } },
            new ClipConfig { clipName = "player_attack3", hitboxDataName = "Player_Attack3_HitboxData", entries = new[] { E(10, "Attack3") } },
            new ClipConfig { clipName = "player_air_attack", hitboxDataName = "Player_AirAttack_HitboxData", entries = new[] {
                E(0, "AirAttack", 0.233f),
                E(22, "AirAttack", 0.15f)
            } },
        };

        // =====================================================================
        //  WINDOW
        // =====================================================================

        //[MenuItem("Roguelite/Hitbox Full Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<HitboxSetupTool>("Hitbox Full Setup");
            window.minSize = new Vector2(620, 700);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Hitbox Data-Driven System — Full Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawSetupButton("1. CREATE ALL HITBOXDATA ASSETS",
                "Creates HitboxData ScriptableObjects for Player, all Enemies (1-5,7), and Boss.",
                "Create All HitboxData Assets",
                CreateAllHitboxData);

            DrawSetupButton("2. ADD HITBOXCONTROLLER TO ALL PREFABS",
                "Adds HitboxController to Player, Enemy1-7, and Boss prefabs. Removes broken EntityHitboxHandler on Boss.",
                "Add HitboxController to All Prefabs",
                AddControllerToAllPrefabs);

            DrawSetupButton("3. ADD ANIMATION EVENTS TO ALL CLIPS",
                "Adds ActivateHitbox(int) / DeactivateHitboxes() events to all attack clips. Removes old m_Enabled curves.",
                "Add Events to All Attack Clips",
                AddEventsToAllClips);

            EditorGUILayout.Space(10);
            DrawSetupButton("\u2605 FULL ONE-CLICK SETUP \u2605",
                "Runs all 3 steps above in sequence. Wait for each to finish.",
                "RUN FULL SETUP (ALL Characters)",
                RunFullSetup,
            new Color(0.15f, 0.5f, 0.15f, 1f));

            EditorGUILayout.Space(20);
            DrawStatusSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSetupButton(string title, string description, string buttonLabel, System.Action action, Color? bgColor = null)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(description, MessageType.Info);
            var defaultColor = GUI.backgroundColor;
            if (bgColor.HasValue) GUI.backgroundColor = bgColor.Value;
            if (GUILayout.Button(buttonLabel, GUILayout.Height(30)))
            {
                EditorApplication.delayCall += () =>
                {
                    try { action(); }
                    catch (System.Exception e) { Debug.LogError($"[HitboxSetup] Error: {e.Message}\n{e.StackTrace}"); }
                };
            }
            GUI.backgroundColor = defaultColor;
            EditorGUILayout.Space(5);
        }

        // =====================================================================
        //  STEP 1: CREATE ALL HITBOXDATA ASSETS
        // =====================================================================

        private void CreateAllHitboxData()
        {
            EnsureHitboxDataFolder();

            // Player
            foreach (var d in PlayerData)
                CreateSingleHitboxData(d.hitboxDataName, ToFrameEntries(d.entries));

            // Enemies + Boss (Enemy7 is shared with Boss)
            foreach (var kvp in EnemyData)
                foreach (var d in kvp.Value)
                    CreateSingleHitboxData(d.hitboxDataName, ToFrameEntries(d.entries));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[HitboxSetup] All HitboxData assets created in {hitboxDataFolder}");
        }

        private void CreateSingleHitboxData(string assetName, HitboxData.HitboxFrameEntry[] entries)
        {
            var path = $"{hitboxDataFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<HitboxData>(path) != null) return;

            var data = ScriptableObject.CreateInstance<HitboxData>();
            data.SetFrameEntries(entries);
            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"[HitboxSetup] Created: {assetName}");
        }

        // =====================================================================
        //  STEP 2: ADD HITBOXCONTROLLER TO ALL PREFABS
        // =====================================================================

        private void AddControllerToAllPrefabs()
        {
            // Player
            var playerPrefabPath = "Assets/Prefabs/Player.prefab";
            if (File.Exists(playerPrefabPath))
                AddControllerToSinglePrefab(playerPrefabPath, "Player", BuildPlayerMappings());
            else
                Debug.LogWarning("[HitboxSetup] Player prefab not found, skipping.");

            // Enemies 1-5,7
            var enemyKeys = new[] { "Enemy1", "Enemy2", "Enemy3", "Enemy4", "Enemy5", "Enemy7" };
            foreach (var key in enemyKeys)
            {
                var path = $"Assets/Prefabs/Enemy/{key}.prefab";
                if (File.Exists(path))
                    AddControllerToSinglePrefab(path, key, BuildEnemyMappings(key));
                else
                    Debug.LogWarning($"[HitboxSetup] {key} prefab not found at {path}, skipping.");
            }

            // Boss
            var bossPath = "Assets/Prefabs/Enemy/Boss.prefab";
            if (File.Exists(bossPath))
            {
                // Boss uses Enemy7 animations
                RemoveBrokenEntityHitboxHandler(bossPath);
                AddControllerToSinglePrefab(bossPath, "Boss", BuildEnemyMappings("Enemy7"));
            }
            else
                Debug.LogWarning("[HitboxSetup] Boss prefab not found, skipping.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HitboxSetup] HitboxController added to all prefabs.");
        }

        private Dictionary<string, string> BuildPlayerMappings()
        {
            return new Dictionary<string, string>
            {
                { "player_attack1", "Player_Attack1_HitboxData" },
                { "player_attack2", "Player_Attack2_HitboxData" },
                { "player_attack3", "Player_Attack3_HitboxData" },
                { "Air Attack", "Player_AirAttack_HitboxData" },
            };
        }

        private Dictionary<string, string> BuildEnemyMappings(string enemyKey)
        {
            var map = new Dictionary<string, string>();
            if (!EnemyData.TryGetValue(enemyKey, out var frames)) return map;

            foreach (var f in frames)
            {
                map[f.clipName] = f.hitboxDataName;
            }
            return map;
        }

        private void AddControllerToSinglePrefab(string prefabPath, string label, Dictionary<string, string> stateToDataMap)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[HitboxSetup] Cannot load {prefabPath}");
                return;
            }

            try
            {
                var existing = prefabRoot.GetComponent<HitboxController>();
                if (existing != null)
                    GameObject.DestroyImmediate(existing);

                var controller = prefabRoot.AddComponent<HitboxController>();
                var serialized = new SerializedObject(controller);
                var dataSetsProp = serialized.FindProperty("hitboxDataSets");
                var animatorProp = serialized.FindProperty("animator");
                if (dataSetsProp == null) { Debug.LogError("hitboxDataSets prop not found"); return; }

                dataSetsProp.ClearArray();
                int idx = 0;
                foreach (var kvp in stateToDataMap)
                {
                    var dataPath = $"{hitboxDataFolder}/{kvp.Value}.asset";
                    var data = AssetDatabase.LoadAssetAtPath<HitboxData>(dataPath);
                    if (data == null) continue;

                    dataSetsProp.InsertArrayElementAtIndex(idx);
                    var el = dataSetsProp.GetArrayElementAtIndex(idx);
                    el.FindPropertyRelative("animationStateName").stringValue = kvp.Key;
                    el.FindPropertyRelative("hitboxData").objectReferenceValue = data;
                    idx++;
                }

                animatorProp.objectReferenceValue = prefabRoot.GetComponent<Animator>();
                serialized.ApplyModifiedProperties();

                // Ensure children start disabled
                foreach (Transform child in prefabRoot.transform)
                {
                    if (child.name.Contains("Attack") || child.name.Contains("AirAttack"))
                        child.gameObject.SetActive(false);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log($"[HitboxSetup] {label}: HitboxController configured ({idx} states).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private void RemoveBrokenEntityHitboxHandler(string prefabPath)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null) return;

            try
            {
                int totalRemoved = 0;
                var allTransforms = prefabRoot.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                    totalRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);

                if (totalRemoved > 0)
                {
                    Debug.Log($"[HitboxSetup] Boss: removed {totalRemoved} missing script(s).");
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        // =====================================================================
        //  STEP 3: ADD ANIMATION EVENTS TO ALL CLIPS
        // =====================================================================

        private void AddEventsToAllClips()
        {
            // Player
            ProcessClipGroup("Assets/Animations/Player", PlayerData);

            // Enemies
            var enemyKeys = new[] { "Enemy1", "Enemy2", "Enemy3", "Enemy4", "Enemy5", "Enemy7" };
            foreach (var key in enemyKeys)
            {
                if (EnemyData.TryGetValue(key, out var frames))
                {
                    var folder = $"Assets/Animations/{key}";
                    if (AssetDatabase.IsValidFolder(folder))
                        ProcessClipGroup(folder, frames);
                    else
                        Debug.LogWarning($"[HitboxSetup] Anim folder not found: {folder}");
                }
            }

            // Boss shares Enemy7 clips, already processed above

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HitboxSetup] All animation events added.");
        }

        private void ProcessClipGroup(string folder, ClipConfig[] clipData)
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folder });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;

                var match = System.Array.Find(clipData, d => d.clipName == clip.name);
                if (match.clipName == null) continue;

                ProcessSingleClip(clip, match);
            }
        }

        private void ProcessSingleClip(AnimationClip clip, ClipConfig config)
        {
            Debug.Log($"[HitboxSetup] Processing clip: {clip.name}");

            // Collect unique child names from all entries for curve removal
            var allChildNames = new HashSet<string>();
            foreach (var e in config.entries)
                if (!string.IsNullOrEmpty(e.childName))
                    allChildNames.Add(e.childName);

            // Remove m_Enabled curves on all hitbox children
            foreach (var cn in allChildNames)
                RemoveEnabledCurves(clip, cn);

            // Disable looping for attack clips
            var serializedClip = new SerializedObject(clip);
            var loopProp = serializedClip.FindProperty("m_LoopTime");
            if (loopProp != null && loopProp.boolValue)
            {
                loopProp.boolValue = false;
                serializedClip.ApplyModifiedProperties();
                Debug.Log($"[HitboxSetup]  Disabled loopTime on '{clip.name}'");
            }

            // Clear old events
            AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);

            float sampleRate = clip.frameRate > 0 ? clip.frameRate : 60f;
            float clipLength = clip.length;
            var events = new List<AnimationEvent>();

            // Activate events for each entry
            foreach (var entry in config.entries)
            {
                float activateTime = Mathf.Clamp(entry.frame / sampleRate, 0f, clipLength);
                events.Add(new AnimationEvent
                {
                    time = activateTime,
                    functionName = "ActivateHitbox",
                    intParameter = entry.frame,
                    messageOptions = SendMessageOptions.DontRequireReceiver
                });
                Debug.Log($"[HitboxSetup]  + ActivateHitbox({entry.frame}) @ {activateTime:F3}s -> {entry.childName}");
            }

            // Safety deactivate at clip end
            float deactivateTime = Mathf.Max(0f, clipLength - 0.016f);
            events.Add(new AnimationEvent
            {
                time = deactivateTime,
                functionName = "DeactivateHitboxes",
                messageOptions = SendMessageOptions.DontRequireReceiver
            });

            AnimationUtility.SetAnimationEvents(clip, events.ToArray());
            EditorUtility.SetDirty(clip);

            Debug.Log($"[HitboxSetup]  {clip.name}: {config.entries.Length} activate(s), DeactivateHitboxes @ {deactivateTime:F3}s");
        }

        private void RemoveEnabledCurves(AnimationClip clip, string childName)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (binding.path == childName && binding.propertyName == "m_Enabled")
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                }
            }
        }

        // =====================================================================
        //  FULL SETUP
        // =====================================================================

        private void RunFullSetup()
        {
            Debug.Log("==============================================");
            Debug.Log("[HitboxSetup] === FULL SETUP STARTED ===");
            Debug.Log("==============================================");

            CreateAllHitboxData();
            AddControllerToAllPrefabs();
            AddEventsToAllClips();

            Debug.Log("==============================================");
            Debug.Log("[HitboxSetup] === FULL SETUP COMPLETE! ===");
            Debug.Log("[HitboxSetup] You can now test in play mode.");
            Debug.Log("==============================================");
        }

        // =====================================================================
        //  STATUS
        // =====================================================================

        private void DrawStatusSection()
        {
            GUILayout.Label("Status Overview", EditorStyles.boldLabel);
            EnsureHitboxDataFolder();

            GUILayout.Label("HitboxData Assets:", EditorStyles.boldLabel);
            CheckAsset("Player_Attack1_HitboxData");
            CheckAsset("Player_Attack2_HitboxData");
            CheckAsset("Player_Attack3_HitboxData");
            CheckAsset("Player_AirAttack_HitboxData");

            foreach (var key in EnemyData.Keys)
            {
                foreach (var d in EnemyData[key])
                    CheckAsset(d.hitboxDataName);
            }

            GUILayout.Label("", EditorStyles.miniLabel);

            GUILayout.Label("Prefabs with HitboxController:", EditorStyles.boldLabel);
            CheckPrefab("Assets/Prefabs/Player.prefab", "Player");
            foreach (var key in new[] { "Enemy1","Enemy2","Enemy3","Enemy4","Enemy5","Enemy7" })
                CheckPrefab($"Assets/Prefabs/Enemy/{key}.prefab", key);
            CheckPrefab("Assets/Prefabs/Enemy/Boss.prefab", "Boss");
        }

        private void CheckAsset(string name)
        {
            var path = $"{hitboxDataFolder}/{name}.asset";
            var exists = AssetDatabase.LoadAssetAtPath<HitboxData>(path) != null;
            GUILayout.Label($"  {(exists ? "\u2714" : "\u2718")} {name}", exists ? EditorStyles.label : EditorStyles.miniLabel);
        }

        private void CheckPrefab(string path, string label)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { GUILayout.Label($"  \u2718 {label} — NOT FOUND"); return; }
            var hc = prefab.GetComponent<HitboxController>();
            GUILayout.Label($"  {(hc != null ? "\u2714" : "\u2718")} {label} — HitboxController: {(hc != null ? "OK" : "MISSING")}");
        }

        // =====================================================================
        //  HELPERS
        // =====================================================================

        private void EnsureHitboxDataFolder()
        {
            if (!AssetDatabase.IsValidFolder(hitboxDataFolder))
            {
                var parent = Path.GetDirectoryName(hitboxDataFolder).Replace("\\", "/");
                var folderName = Path.GetFileName(hitboxDataFolder);
                if (AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
