using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Roguelite.UpgradeSystem.Editor
{
    /// <summary>
    /// Công cụ Unity Editor tạo mẫu và quản lý các ScriptableObject Nâng Cấp Vĩnh Viễn (Permanent Upgrades).
    /// Mở từ Menu Unity: Tools -> Roguelite -> Permanent Upgrade Generator.
    /// </summary>
    public class PermanentUpgradeGeneratorWindow : EditorWindow
    {
        private PermanentUpgradeDatabase database;
        private Vector2 scrollPos;

        private GUIStyle headerStyle;
        private GUIStyle cardStyle;
        private GUIStyle titleStyle;
        private bool stylesInitialized = false;

        [MenuItem("Tools/Roguelite/Permanent Upgrade Generator", false, 11)]
        public static void ShowWindow()
        {
            var window = GetWindow<PermanentUpgradeGeneratorWindow>("Permanent Upgrade Tool");
            window.minSize = new Vector2(720, 520);
            window.Show();
        }

        private void OnEnable()
        {
            FindOrCreateDatabase();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft
            };

            cardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };

            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.2f, 0.85f, 0.4f) }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(8);
            GUILayout.Label("💎 PERMANENT UPGRADE GENERATOR & EDITOR", headerStyle);
            EditorGUILayout.LabelField("Công cụ sinh mẫu 5 Nâng Cấp Vĩnh Viễn (Multi-tier, Milestones) và quản lý Database.", EditorStyles.miniLabel);
            EditorGUILayout.Space(6);

            // --- 1. DATABASE HEADER ---
            DrawDatabaseHeader();

            EditorGUILayout.Space(6);

            // --- 2. ACTION BUTTONS ---
            DrawActionButtons();

            EditorGUILayout.Space(6);

            // --- 3. UPGRADES LIST ---
            DrawUpgradesList();
        }

        private void DrawDatabaseHeader()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("🗄️ PERMANENT UPGRADE DATABASE", titleStyle);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            database = (PermanentUpgradeDatabase)EditorGUILayout.ObjectField("Database Asset", database, typeof(PermanentUpgradeDatabase), false);

            if (GUILayout.Button("🔄 Quét DB", GUILayout.Width(90), GUILayout.Height(20)))
            {
                FindOrCreateDatabase();
                ScanAndSyncUpgrades();
            }

            if (GUILayout.Button("🎁 Sinh 5 Upgrade Mẫu", GUILayout.Width(150), GUILayout.Height(20)))
            {
                FindOrCreateDatabase();
                Generate5SampleUpgrades();
            }
            EditorGUILayout.EndHorizontal();

            if (database != null)
            {
                EditorGUILayout.HelpBox($"Database đang quản lý [{database.AllUpgrades.Count}] Nâng cấp vĩnh viễn trong dự án.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa chọn PermanentUpgradeDatabase! Nhấn 'Quét DB' hoặc 'Sinh 5 Upgrade Mẫu' để khởi tạo.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("⚡ THAO TÁC TỰ ĐỘNG", titleStyle);
            EditorGUILayout.Space(4);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("✨ TỰ ĐỘNG SINH & ĐỒNG BỘ 5 NÂNG CẤP VĨNH VIỄN MẪU ✨", GUILayout.Height(32)))
            {
                FindOrCreateDatabase();
                Generate5SampleUpgrades();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawUpgradesList()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("📜 DANH SÁCH PERMANENT UPGRADES TRONG DATABASE", titleStyle);
            EditorGUILayout.Space(4);

            if (database == null || database.AllUpgrades == null || database.AllUpgrades.Count == 0)
            {
                EditorGUILayout.HelpBox("Danh sách nâng cấp trống. Nhấn nút 'Sinh 5 Upgrade Mẫu' ở trên để tự động khởi tạo.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            for (int i = 0; i < database.AllUpgrades.Count; i++)
            {
                PermanentUpgradeData upgrade = database.AllUpgrades[i];
                if (upgrade == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"[{i}] UpgradeData bị Null");
                    if (GUILayout.Button("Xóa khỏi DB", GUILayout.Width(90)))
                    {
                        database.AllUpgrades.RemoveAt(i);
                        EditorUtility.SetDirty(database);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                Texture2D iconTex = upgrade.Icon != null ? AssetPreview.GetAssetPreview(upgrade.Icon) : null;
                if (iconTex != null)
                {
                    GUILayout.Label(iconTex, GUILayout.Width(40), GUILayout.Height(40));
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"<b>[{upgrade.UpgradeId}]</b> {upgrade.UpgradeName} <color=#ffcc00>({upgrade.Category})</color>", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Cấp tối đa: <color=#00e5ff>{upgrade.MaxLevel} Tiers</color> | Mô tả: {upgrade.Description}", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("🔍 Chọn Asset", GUILayout.Width(90), GUILayout.Height(28)))
                {
                    Selection.activeObject = upgrade;
                    EditorGUIUtility.PingObject(upgrade);
                }

                if (GUILayout.Button("🗑️ Xóa", GUILayout.Width(55), GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("Xác nhận", $"Xóa '{upgrade.UpgradeName}' khỏi Database?", "Xóa", "Hủy"))
                    {
                        database.AllUpgrades.RemoveAt(i);
                        EditorUtility.SetDirty(database);
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void FindOrCreateDatabase()
        {
            string folderPath = "Assets/ScriptableObject";
            string assetPath = $"{folderPath}/PermanentUpgradeDatabase.asset";

            database = AssetDatabase.LoadAssetAtPath<PermanentUpgradeDatabase>(assetPath);

            if (database == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:PermanentUpgradeDatabase");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    database = AssetDatabase.LoadAssetAtPath<PermanentUpgradeDatabase>(path);
                }
            }

            if (database == null)
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    AssetDatabase.Refresh();
                }

                database = ScriptableObject.CreateInstance<PermanentUpgradeDatabase>();
                AssetDatabase.CreateAsset(database, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[PermanentUpgradeGenerator] Đã khởi tạo PermanentUpgradeDatabase tại: {assetPath}");
            }
        }

        private void ScanAndSyncUpgrades()
        {
            if (database == null) FindOrCreateDatabase();

            string[] guids = AssetDatabase.FindAssets("t:PermanentUpgradeData");
            int addedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PermanentUpgradeData upgrade = AssetDatabase.LoadAssetAtPath<PermanentUpgradeData>(path);
                if (upgrade != null && !database.AllUpgrades.Contains(upgrade))
                {
                    database.AllUpgrades.Add(upgrade);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                Debug.Log($"[PermanentUpgradeGenerator] Đã đồng bộ thêm {addedCount} Nâng cấp vào Database!");
            }
        }

        private void Generate5SampleUpgrades()
        {
            string folderPath = "Assets/ScriptableObject/PermanentUpgradeData";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // Cấu trúc 5 Upgrade mẫu
            var samples = new[]
            {
                new {
                    id = "perm_max_health",
                    name = "Sinh Lực Thần Thánh",
                    desc = "Tăng chỉ số Máu Tối Đa (Max HP) vĩnh viễn qua các lượt run.",
                    cat = PermanentUpgradeCategory.Defense,
                    stat = PlayerStatType.MaxHealth,
                    baseCost = 100,
                    costInc = 100,
                    valPerTier = 15f,
                    isPerc = false,
                    milestoneVal = 100f,
                    milestoneBonusVal = 0.15f, // +15% HP
                    milestoneBonusIsPerc = true,
                    milestoneDesc = "Máu Bất Tử: +15% Max HP tổng!",
                    kills = 0, runs = 0, rooms = 0, def = true
                },
                new {
                    id = "perm_attack_damage",
                    name = "Cường Lực Chiến Tướng",
                    desc = "Gia tăng Sát Thương Đòn Đánh (Attack Damage) vĩnh viễn.",
                    cat = PermanentUpgradeCategory.Offense,
                    stat = PlayerStatType.AttackDamage,
                    baseCost = 120,
                    costInc = 120,
                    valPerTier = 5f,
                    isPerc = false,
                    milestoneVal = 30f,
                    milestoneBonusVal = 0.20f, // +20% Damage
                    milestoneBonusIsPerc = true,
                    milestoneDesc = "Cuồng Nộ Báo Thù: +20% Sát thương tổng!",
                    kills = 20, runs = 1, rooms = 2, def = false
                },
                new {
                    id = "perm_walk_speed",
                    name = "Bộ Hành Thần Tốc",
                    desc = "Tăng tốc độ di chuyển đi bộ (Walk Speed) vĩnh viễn.",
                    cat = PermanentUpgradeCategory.Utility,
                    stat = PlayerStatType.WalkSpeed,
                    baseCost = 80,
                    costInc = 80,
                    valPerTier = 0.5f,
                    isPerc = false,
                    milestoneVal = 3.0f,
                    milestoneBonusVal = 0.10f, // +10% Walk Speed
                    milestoneBonusIsPerc = true,
                    milestoneDesc = "Bước Chân Phong Thần: +10% Tốc độ bước!",
                    kills = 10, runs = 1, rooms = 0, def = false
                },
                new {
                    id = "perm_run_speed",
                    name = "Bứt Tốc Truy Kích",
                    desc = "Tăng tốc độ di chuyển chạy nhanh (Run Speed) vĩnh viễn.",
                    cat = PermanentUpgradeCategory.Utility,
                    stat = PlayerStatType.RunSpeed,
                    baseCost = 90,
                    costInc = 90,
                    valPerTier = 0.8f,
                    isPerc = false,
                    milestoneVal = 4.5f,
                    milestoneBonusVal = 0.15f, // +15% Run Speed
                    milestoneBonusIsPerc = true,
                    milestoneDesc = "Phi Lôi Thần: +15% Tốc độ chạy nhanh!",
                    kills = 30, runs = 2, rooms = 3, def = false
                },
                new {
                    id = "perm_jump_impulse",
                    name = "Bật Cao Phi Thường",
                    desc = "Tăng lực nhảy (Jump Impulse) giúp bật cao và né né kẻ địch linh hoạt hơn.",
                    cat = PermanentUpgradeCategory.Utility,
                    stat = PlayerStatType.JumpImpulse,
                    baseCost = 100,
                    costInc = 100,
                    valPerTier = 1.0f,
                    isPerc = false,
                    milestoneVal = 6.0f,
                    milestoneBonusVal = 0.20f, // +20% Jump
                    milestoneBonusIsPerc = true,
                    milestoneDesc = "Đôi Cánh Tự Do: +20% Lực bật nhảy!",
                    kills = 50, runs = 3, rooms = 4, def = false
                }
            };

            int createdCount = 0;

            foreach (var s in samples)
            {
                string assetPath = $"{folderPath}/Upgrade_{s.id}.asset";
                PermanentUpgradeData upgrade = AssetDatabase.LoadAssetAtPath<PermanentUpgradeData>(assetPath);

                if (upgrade == null)
                {
                    upgrade = ScriptableObject.CreateInstance<PermanentUpgradeData>();
                    AssetDatabase.CreateAsset(upgrade, assetPath);
                    createdCount++;
                }

                SerializedObject so = new SerializedObject(upgrade);
                so.FindProperty("upgradeId").stringValue = s.id;
                so.FindProperty("upgradeName").stringValue = s.name;
                so.FindProperty("description").stringValue = s.desc;
                so.FindProperty("category").enumValueIndex = (int)s.cat;
                so.FindProperty("requiredEnemiesKilled").intValue = s.kills;
                so.FindProperty("requiredRunsPlayed").intValue = s.runs;
                so.FindProperty("requiredHighestRoom").intValue = s.rooms;
                so.FindProperty("isDefaultUnlocked").boolValue = s.def;

                // Xây dựng 5 Tiers
                SerializedProperty tiersProp = so.FindProperty("tiers");
                tiersProp.ClearArray();

                for (int tierIdx = 1; tierIdx <= 5; tierIdx++)
                {
                    tiersProp.InsertArrayElementAtIndex(tierIdx - 1);
                    SerializedProperty tierElem = tiersProp.GetArrayElementAtIndex(tierIdx - 1);

                    tierElem.FindPropertyRelative("tierIndex").intValue = tierIdx;
                    tierElem.FindPropertyRelative("cost").intValue = s.baseCost * tierIdx;
                    tierElem.FindPropertyRelative("statType").enumValueIndex = (int)s.stat;
                    tierElem.FindPropertyRelative("isPercent").boolValue = s.isPerc;

                    if (tierIdx == 5)
                    {
                        // Milestone Tier (Cấp 5)
                        tierElem.FindPropertyRelative("statValue").floatValue = s.milestoneVal;
                        tierElem.FindPropertyRelative("isMilestone").boolValue = true;

                        SerializedProperty milestoneProp = tierElem.FindPropertyRelative("milestoneBonus");
                        milestoneProp.FindPropertyRelative("bonusDescription").stringValue = s.milestoneDesc;
                        milestoneProp.FindPropertyRelative("statType").enumValueIndex = (int)s.stat;
                        milestoneProp.FindPropertyRelative("statValue").floatValue = s.milestoneBonusVal;
                        milestoneProp.FindPropertyRelative("isPercent").boolValue = s.milestoneBonusIsPerc;
                        milestoneProp.FindPropertyRelative("specialBehaviorKey").stringValue = $"{s.id}_milestone_tier_5";
                    }
                    else
                    {
                        // Normal Tier
                        tierElem.FindPropertyRelative("statValue").floatValue = s.valPerTier * tierIdx;
                        tierElem.FindPropertyRelative("isMilestone").boolValue = false;
                    }
                }

                so.ApplyModifiedProperties();

                if (database != null && !database.AllUpgrades.Contains(upgrade))
                {
                    database.AllUpgrades.Add(upgrade);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (database != null)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }

            EditorUtility.DisplayDialog("Tạo Nâng Cấp Vĩnh Viễn Mẫu", $"Đã tạo thành công {samples.Length} Permanent Upgrades (mỗi cái 5 cấp độ & Milestone) và đồng bộ vào Database!", "OK");
        }
    }
}
