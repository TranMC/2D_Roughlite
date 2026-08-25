using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Roguelite.Combat.Editor
{
    /// <summary>
    /// Cửa sổ Editor nâng cao dành cho việc Quản lý Vũ khí (Weapons), Cửa Hàng (Shop Price) và Điều Kiện Mở Khóa (Requirements).
    /// Mở từ Menu Unity: Tools -> Roguelite -> Weapon & Shop Editor.
    /// </summary>
    public class WeaponEditorWindow : EditorWindow
    {
        private WeaponDatabase database;
        private Vector2 scrollPos;
        private Vector2 createScrollPos;

        // Form Tạo Vũ Khí Mới
        private bool showCreatePanel = true;
        private string newWeaponId = "sword_fire";
        private string newWeaponName = "Thanh Kiếm Lửa";
        private Sprite newIcon;
        private string newDescription = "Thanh kiếm mang sức mạnh rực cháy của ngọn lửa cổ đại.";
        private float newDamage = 25f;
        private float newAttackSpeed = 1.2f;
        private float newRange = 1.8f;
        private int newPrice = 250;
        private int newRequiredKills = 20;
        private int newRequiredRuns = 3;
        private int newRequiredHighestRoom = 5;
        private bool newIsDefaultUnlocked = false;

        private GUIStyle headerStyle;
        private GUIStyle cardStyle;
        private GUIStyle titleStyle;
        private bool stylesInitialized = false;

        [MenuItem("Tools/Roguelite/Weapon & Shop Editor", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<WeaponEditorWindow>("Weapon Editor");
            window.minSize = new Vector2(750, 550);
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
                normal = { textColor = new Color(0f, 0.85f, 1f) }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(8);
            GUILayout.Label("⚔️ WEAPON & SHOP DATABASE EDITOR", headerStyle);
            EditorGUILayout.LabelField("Công cụ quản lý toàn bộ Vũ khí, Giá bán trong Cửa hàng & Điều kiện mở khóa theo thành tựu.", EditorStyles.miniLabel);
            EditorGUILayout.Space(6);

            // --- 1. DATABASE HEADER ---
            DrawDatabaseHeader();

            EditorGUILayout.Space(6);

            // --- 2. CREATE NEW WEAPON FORM ---
            DrawCreateWeaponForm();

            EditorGUILayout.Space(6);

            // --- 3. LIST ALL WEAPONS ---
            DrawWeaponList();
        }

        private void DrawDatabaseHeader()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("🗄️ DATABASE CONFIGURATION", titleStyle);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            database = (WeaponDatabase)EditorGUILayout.ObjectField("Weapon Database", database, typeof(WeaponDatabase), false);

            if (GUILayout.Button("🔄 Quét DB", GUILayout.Width(90), GUILayout.Height(20)))
            {
                FindOrCreateDatabase();
                ScanAndSyncWeapons();
            }

            if (GUILayout.Button("🎁 Sinh 10 Vũ Khí Mẫu", GUILayout.Width(140), GUILayout.Height(20)))
            {
                FindOrCreateDatabase();
                Generate10SampleWeapons();
            }
            EditorGUILayout.EndHorizontal();

            if (database != null)
            {
                EditorGUILayout.HelpBox($"Database đang quản lý [{database.AllWeapons.Count}] vũ khí trong dự án.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa gán WeaponDatabase! Nhấn nút 'Quét DB' hoặc 'Sinh 10 Vũ Khí Mẫu' để khởi tạo tự động.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCreateWeaponForm()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            showCreatePanel = EditorGUILayout.Foldout(showCreatePanel, "➕ TẠO VŨ KHÍ MỚI (CREATE NEW WEAPON DATA)", true, EditorStyles.foldoutHeader);

            if (showCreatePanel)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                
                // Cột trái: Thông tin cơ bản
                EditorGUILayout.BeginVertical(GUILayout.Width(350));
                newWeaponId = EditorGUILayout.TextField("Weapon ID", newWeaponId);
                newWeaponName = EditorGUILayout.TextField("Weapon Name", newWeaponName);
                newIcon = (Sprite)EditorGUILayout.ObjectField("Icon Sprite", newIcon, typeof(Sprite), false, GUILayout.Height(18));
                newDescription = EditorGUILayout.TextField("Mô tả", newDescription);
                newDamage = EditorGUILayout.FloatField("Sát thương (Damage)", newDamage);
                newAttackSpeed = EditorGUILayout.FloatField("Tốc đánh (Speed)", newAttackSpeed);
                newRange = EditorGUILayout.FloatField("Tầm đánh (Range)", newRange);
                EditorGUILayout.EndVertical();

                GUILayout.Space(10);

                // Cột phải: Shop & Unlock Requirements
                EditorGUILayout.BeginVertical();
                GUILayout.Label("<b>Shop & Lock Requirements</b>", EditorStyles.label);
                newPrice = EditorGUILayout.IntField("Giá bán (Gold)", newPrice);
                newRequiredKills = EditorGUILayout.IntField("Yêu cầu diệt Quái", newRequiredKills);
                newRequiredRuns = EditorGUILayout.IntField("Yêu cầu số Lượt Run", newRequiredRuns);
                newRequiredHighestRoom = EditorGUILayout.IntField("Yêu cầu Cấp Phòng", newRequiredHighestRoom);
                newIsDefaultUnlocked = EditorGUILayout.Toggle("Mở khóa Mặc Định", newIsDefaultUnlocked);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
                if (GUILayout.Button("✨ Tạo ScriptableObject Vũ Khí Mới", GUILayout.Height(28)))
                {
                    CreateNewWeaponAsset();
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWeaponList()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("📜 DANH SÁCH VŨ KHÍ TRONG DATABASE", titleStyle);
            EditorGUILayout.Space(4);

            if (database == null || database.AllWeapons == null || database.AllWeapons.Count == 0)
            {
                EditorGUILayout.HelpBox("Danh sách vũ khí trống.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            for (int i = 0; i < database.AllWeapons.Count; i++)
            {
                WeaponData weapon = database.AllWeapons[i];
                if (weapon == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"[{i}] WeaponData bị Null");
                    if (GUILayout.Button("Xóa khỏi DB", GUILayout.Width(90)))
                    {
                        database.AllWeapons.RemoveAt(i);
                        EditorUtility.SetDirty(database);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                SerializedObject serializedWeapon = new SerializedObject(weapon);
                serializedWeapon.Update();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                // Preview Icon
                Texture2D iconTex = weapon.Icon != null ? AssetPreview.GetAssetPreview(weapon.Icon) : null;
                if (iconTex != null)
                {
                    GUILayout.Label(iconTex, GUILayout.Width(40), GUILayout.Height(40));
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"<b>[{weapon.WeaponId}]</b> - {weapon.WeaponName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Dam: <color=#ff4d4d>{weapon.Damage}</color> | Spd: <color=#00e5ff>{weapon.AttackSpeed}</color> | Price: <color=#ffcc00>{weapon.Price} G</color> | Req: Kills({weapon.RequiredEnemiesKilled}), Runs({weapon.RequiredRunsPlayed}), Room({weapon.RequiredHighestRoom})", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("🔍 Chọn Asset", GUILayout.Width(90), GUILayout.Height(30)))
                {
                    Selection.activeObject = weapon;
                    EditorGUIUtility.PingObject(weapon);
                }

                if (GUILayout.Button("🗑️ Xóa", GUILayout.Width(55), GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Xác nhận xóa", $"Bạn có chắc muốn bỏ vũ khí '{weapon.WeaponName}' khỏi Database?", "Xóa", "Hủy"))
                    {
                        database.AllWeapons.RemoveAt(i);
                        EditorUtility.SetDirty(database);
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                serializedWeapon.ApplyModifiedProperties();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void FindOrCreateDatabase()
        {
            string folderPath = "Assets/ScriptableObjects/Weapons";
            string assetPath = $"{folderPath}/WeaponDatabase.asset";

            database = AssetDatabase.LoadAssetAtPath<WeaponDatabase>(assetPath);

            if (database == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:WeaponDatabase");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    database = AssetDatabase.LoadAssetAtPath<WeaponDatabase>(path);
                }
            }

            if (database == null)
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    AssetDatabase.Refresh();
                }

                database = ScriptableObject.CreateInstance<WeaponDatabase>();
                AssetDatabase.CreateAsset(database, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[WeaponEditorWindow] Đã khởi tạo mới WeaponDatabase tại: {assetPath}");
            }
        }

        private void ScanAndSyncWeapons()
        {
            if (database == null) FindOrCreateDatabase();

            string[] guids = AssetDatabase.FindAssets("t:WeaponData");
            int addedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (weapon != null && !database.AllWeapons.Contains(weapon))
                {
                    database.AllWeapons.Add(weapon);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                Debug.Log($"[WeaponEditorWindow] Đã đồng bộ thêm {addedCount} vũ khí vào WeaponDatabase!");
            }
            else
            {
                Debug.Log("[WeaponEditorWindow] Database đã đồng bộ đầy đủ toàn bộ vũ khí.");
            }
        }

        private void CreateNewWeaponAsset()
        {
            if (string.IsNullOrWhiteSpace(newWeaponId))
            {
                EditorUtility.DisplayDialog("Lỗi", "Weapon ID không được để trống!", "OK");
                return;
            }

            string folderPath = "Assets/ScriptableObjects/Weapons";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            string fileName = $"Weapon_{newWeaponId}.asset";
            string fullPath = $"{folderPath}/{fileName}";

            if (File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("Lỗi", $"File vũ khí '{fileName}' đã tồn tại tại {fullPath}!", "OK");
                return;
            }

            WeaponData newWeapon = ScriptableObject.CreateInstance<WeaponData>();
            
            // Dùng SerializedObject để set dữ liệu
            SerializedObject so = new SerializedObject(newWeapon);
            so.FindProperty("weaponId").stringValue = newWeaponId;
            so.FindProperty("weaponName").stringValue = newWeaponName;
            so.FindProperty("icon").objectReferenceValue = newIcon;
            so.FindProperty("description").stringValue = newDescription;
            so.FindProperty("damage").floatValue = newDamage;
            so.FindProperty("attackSpeed").floatValue = newAttackSpeed;
            so.FindProperty("range").floatValue = newRange;
            so.FindProperty("price").intValue = newPrice;
            so.FindProperty("requiredEnemiesKilled").intValue = newRequiredKills;
            so.FindProperty("requiredRunsPlayed").intValue = newRequiredRuns;
            so.FindProperty("requiredHighestRoom").intValue = newRequiredHighestRoom;
            so.FindProperty("isDefaultUnlocked").boolValue = newIsDefaultUnlocked;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(newWeapon, fullPath);
            AssetDatabase.SaveAssets();

            // Đưa vào Database
            if (database != null)
            {
                if (!database.AllWeapons.Contains(newWeapon))
                {
                    database.AllWeapons.Add(newWeapon);
                    EditorUtility.SetDirty(database);
                    AssetDatabase.SaveAssets();
                }
            }

            Selection.activeObject = newWeapon;
            EditorUtility.DisplayDialog("Thành công", $"Đã tạo thành công vũ khí '{newWeaponName}' ({fileName})!", "OK");
        }

        private void Generate10SampleWeapons()
        {
            string folderPath = "Assets/ScriptableObject/WeaponData";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            var samples = new[]
            {
                new { id = "sword_starter", name = "Kiếm Tập Sự", desc = "Vũ khí cơ bản dành cho các tân binh bắt đầu hành trình.", dam = 12f, spd = 1.0f, range = 1.5f, price = 0, kills = 0, runs = 0, rooms = 0, def = true },
                new { id = "sword_steel", name = "Kiếm Thép Rèn", desc = "Lưỡi kiếm bằng thép tinh luyện, sát thương ổn định và sắc bén.", dam = 18f, spd = 1.1f, range = 1.6f, price = 150, kills = 15, runs = 1, rooms = 2, def = false },
                new { id = "dagger_wind", name = "Song Đao Cuồng Phong", desc = "Cặp dao ngắn xé gió, tốc độ tấn công cực nhanh.", dam = 10f, spd = 1.8f, range = 1.2f, price = 250, kills = 30, runs = 2, rooms = 3, def = false },
                new { id = "greatsword_avenger", name = "Đại Đao Báo Thù", desc = "Thanh đại đao nặng nề với sát thương diện rộng bộc phá.", dam = 32f, spd = 0.75f, range = 2.2f, price = 400, kills = 50, runs = 3, rooms = 5, def = false },
                new { id = "axe_berserker", name = "Rìu Chiến Berserker", desc = "Chiếc rìu chứa đựng sức mạnh dũng mãnh của chiến binh dã man.", dam = 26f, spd = 0.9f, range = 1.8f, price = 350, kills = 40, runs = 3, rooms = 4, def = false },
                new { id = "spear_dragon", name = "Thánh Thương Long Nanh", desc = "Cây ngọn thương có tầm đâm xa, dễ dàng hạ gục kẻ địch từ cự ly an toàn.", dam = 22f, spd = 1.15f, range = 2.5f, price = 500, kills = 75, runs = 4, rooms = 6, def = false },
                new { id = "katana_hellfire", name = "Huyết Kiếm Hỏa Ngục", desc = "Lưỡi kiếm ma thuật bùng cháy lửa đỏ, chém tan mọi rào cản.", dam = 28f, spd = 1.3f, range = 1.7f, price = 650, kills = 100, runs = 5, rooms = 7, def = false },
                new { id = "hammer_frost", name = "Búa Sấm Băng Nham", desc = "Cây búa hất văng kẻ thù với lực chấn động cực mạnh.", dam = 38f, spd = 0.65f, range = 2.0f, price = 800, kills = 150, runs = 6, rooms = 8, def = false },
                new { id = "claw_shadow", name = "Vuốt Ma Bóng Đêm", desc = "Vuốt quỷ liên kích thần tốc, gieo rắc nỗi sợ hãi chớp nát kẻ địch.", dam = 16f, spd = 1.6f, range = 1.3f, price = 550, kills = 80, runs = 5, rooms = 6, def = false },
                new { id = "sword_excalibur", name = "Huyền Thoại Quang Kiếm", desc = "Vũ khí tối thượng phát ra hào quang thánh linh, quét sạch mọi bóng tối.", dam = 45f, spd = 1.4f, range = 2.4f, price = 1200, kills = 250, runs = 10, rooms = 10, def = false }
            };

            int createdCount = 0;

            foreach (var s in samples)
            {
                string assetPath = $"{folderPath}/Weapon_{s.id}.asset";
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);

                if (weapon == null)
                {
                    weapon = ScriptableObject.CreateInstance<WeaponData>();
                    AssetDatabase.CreateAsset(weapon, assetPath);
                    createdCount++;
                }

                SerializedObject so = new SerializedObject(weapon);
                so.FindProperty("weaponId").stringValue = s.id;
                so.FindProperty("weaponName").stringValue = s.name;
                so.FindProperty("description").stringValue = s.desc;
                so.FindProperty("damage").floatValue = s.dam;
                so.FindProperty("attackSpeed").floatValue = s.spd;
                so.FindProperty("range").floatValue = s.range;
                so.FindProperty("price").intValue = s.price;
                so.FindProperty("requiredEnemiesKilled").intValue = s.kills;
                so.FindProperty("requiredRunsPlayed").intValue = s.runs;
                so.FindProperty("requiredHighestRoom").intValue = s.rooms;
                so.FindProperty("isDefaultUnlocked").boolValue = s.def;
                so.ApplyModifiedProperties();

                if (database != null && !database.AllWeapons.Contains(weapon))
                {
                    database.AllWeapons.Add(weapon);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (database != null)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }

            EditorUtility.DisplayDialog("Tạo Vũ Khí Mẫu", $"Đã tạo và đồng bộ {samples.Length} vũ khí mẫu vào WeaponDatabase!", "OK");
        }
    }
}
