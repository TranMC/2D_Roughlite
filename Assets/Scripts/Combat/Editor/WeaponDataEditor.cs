using UnityEditor;
using UnityEngine;

namespace Roguelite.Combat.Editor
{
    /// <summary>
    /// Custom Inspector trực quan, đẹp mắt cho ScriptableObject WeaponData trong Unity Inspector.
    /// </summary>
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : UnityEditor.Editor
    {
        private SerializedProperty weaponIdProp;
        private SerializedProperty weaponNameProp;
        private SerializedProperty iconProp;
        private SerializedProperty descriptionProp;

        private SerializedProperty damageProp;
        private SerializedProperty attackSpeedProp;
        private SerializedProperty rangeProp;
        private SerializedProperty knockbackProp;

        private SerializedProperty priceProp;
        private SerializedProperty requiredEnemiesKilledProp;
        private SerializedProperty requiredRunsPlayedProp;
        private SerializedProperty requiredHighestRoomProp;
        private SerializedProperty isDefaultUnlockedProp;

        private void OnEnable()
        {
            weaponIdProp = serializedObject.FindProperty("weaponId");
            weaponNameProp = serializedObject.FindProperty("weaponName");
            iconProp = serializedObject.FindProperty("icon");
            descriptionProp = serializedObject.FindProperty("description");

            damageProp = serializedObject.FindProperty("damage");
            attackSpeedProp = serializedObject.FindProperty("attackSpeed");
            rangeProp = serializedObject.FindProperty("range");
            knockbackProp = serializedObject.FindProperty("knockback");

            priceProp = serializedObject.FindProperty("price");
            requiredEnemiesKilledProp = serializedObject.FindProperty("requiredEnemiesKilled");
            requiredRunsPlayedProp = serializedObject.FindProperty("requiredRunsPlayed");
            requiredHighestRoomProp = serializedObject.FindProperty("requiredHighestRoom");
            isDefaultUnlockedProp = serializedObject.FindProperty("isDefaultUnlocked");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            WeaponData weapon = (WeaponData)target;

            // Header Banner
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("box");
            if (weapon.Icon != null)
            {
                Texture2D iconTex = AssetPreview.GetAssetPreview(weapon.Icon);
                if (iconTex != null)
                {
                    GUILayout.Label(iconTex, GUILayout.Width(48), GUILayout.Height(48));
                }
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"<b>{weapon.WeaponName}</b> ({weapon.WeaponId})", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, richText = true });
            string lockStatus = weapon.IsDefaultUnlocked ? "<color=#00ff88>✓ Mở khóa mặc định</color>" : $"<color=#ffcc00>🛒 {weapon.Price} Vàng</color>";
            EditorGUILayout.LabelField($"Trạng thái: {lockStatus}", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Group 1: Basic Info
            EditorGUILayout.LabelField("ℹ️ THÔNG TIN CƠ BẢN", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(weaponIdProp, new GUIContent("Weapon ID"));
            EditorGUILayout.PropertyField(weaponNameProp, new GUIContent("Tên Vũ Khí"));
            EditorGUILayout.PropertyField(iconProp, new GUIContent("Icon Sprite"));
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Mô Tả"));

            EditorGUILayout.Space(8);

            // Group 2: Combat Stats
            EditorGUILayout.LabelField("⚔️ CHỈ SỐ CHIẾN ĐẤU", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(damageProp, new GUIContent("Sát Thương (Damage)"));
            EditorGUILayout.PropertyField(attackSpeedProp, new GUIContent("Tốc Độ Đánh (Attack Speed)"));
            EditorGUILayout.PropertyField(rangeProp, new GUIContent("Tầm Đánh (Range)"));
            EditorGUILayout.PropertyField(knockbackProp, new GUIContent("Lực Đẩy Lùi (Knockback)"));

            EditorGUILayout.Space(8);

            // Group 3: Shop & Unlock Requirements
            EditorGUILayout.LabelField("🛒 CỬA HÀNG & ĐIỀU KIỆN MỞ KHÓA", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(priceProp, new GUIContent("Giá Mua (Vàng/Gold)"));
            EditorGUILayout.PropertyField(isDefaultUnlockedProp, new GUIContent("Mở Khóa Mặc Định (Starter)"));

            if (!isDefaultUnlockedProp.boolValue)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("<b>Điều kiện đạt thành tựu để mở bán:</b>", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(requiredEnemiesKilledProp, new GUIContent("Yêu Cầu Số Quái Tiêu Diệt"));
                EditorGUILayout.PropertyField(requiredRunsPlayedProp, new GUIContent("Yêu Cầu Số Lượt Play (Runs)"));
                EditorGUILayout.PropertyField(requiredHighestRoomProp, new GUIContent("Yêu Cầu Cấp Phòng Sâu Nhất"));
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
