using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Roguelite.Combat.Editor
{
    public class AttackConfigGenerator : EditorWindow
    {
        private string sourceFolder = "Assets/Animations/Player";
        private string outputFolder = "Assets/AttackConfigs";
        private Vector2 scrollPos;

        private struct DetectedClip
        {
            public AnimationClip clip;
            public float activateTime;
            public float deactivateTime;
            public bool hasEvents;
        }

        private List<DetectedClip> detectedClips = new List<DetectedClip>();

        //[MenuItem("Roguelite/Generate Attack Configs")]
        public static void ShowWindow()
        {
            var window = GetWindow<AttackConfigGenerator>("Attack Config Generator");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Attack Config Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            sourceFolder = EditorGUILayout.TextField("Source Anim Folder", sourceFolder);
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

            EditorGUILayout.Space();

            if (GUILayout.Button("1. Scan Animation Clips", GUILayout.Height(30)))
                ScanClips();

            EditorGUILayout.Space();

            if (detectedClips.Count > 0)
            {
                GUILayout.Label("Detected Clips:", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));

                foreach (var dc in detectedClips)
                {
                    string status = dc.hasEvents
                        ? $"Activate @ {dc.activateTime:F3}s  Deactivate @ {dc.deactivateTime:F3}s"
                        : "No events found";
                    GUILayout.Label($"  {(dc.clip != null ? dc.clip.name : "?")}: {status}");
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.Space();

                if (GUILayout.Button("2. Generate AttackConfig Assets", GUILayout.Height(30)))
                    GenerateConfigs();
            }

            EditorGUILayout.Space();
            DrawStatus();
        }

        private void ScanClips()
        {
            detectedClips.Clear();

            if (!AssetDatabase.IsValidFolder(sourceFolder))
            {
                Debug.LogError($"Folder not found: {sourceFolder}");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { sourceFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;

                float act = -1f, deact = -1f;
                foreach (var evt in AnimationUtility.GetAnimationEvents(clip))
                {
                    if (evt.functionName == "ActivateHitbox")
                        act = evt.time;
                    else if (evt.functionName == "DeactivateHitboxes")
                        deact = evt.time;
                }

                detectedClips.Add(new DetectedClip
                {
                    clip = clip,
                    activateTime = act >= 0f ? act : 0f,
                    deactivateTime = deact >= 0f ? deact : clip.length - 0.016f,
                    hasEvents = act >= 0f
                });
            }

            Debug.Log($"[AttackConfigGen] Scanned {detectedClips.Count} clips.");
        }

        private void GenerateConfigs()
        {
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                var parent = Path.GetDirectoryName(outputFolder).Replace("\\", "/");
                var name = Path.GetFileName(outputFolder);
                if (!AssetDatabase.IsValidFolder(parent)) AssetDatabase.CreateFolder("Assets", "AttackConfigs");
                else AssetDatabase.CreateFolder(parent, name);
                AssetDatabase.Refresh();
            }

            foreach (var dc in detectedClips)
            {
                if (dc.clip == null || !dc.hasEvents) continue;

                var assetName = ClipToPatternName(dc.clip.name);
                var path = $"{outputFolder}/{assetName}.asset";
                if (AssetDatabase.LoadAssetAtPath<AttackConfig>(path) != null) continue;

                var config = ScriptableObject.CreateInstance<AttackConfig>();
                config.SetTiming(dc.activateTime, dc.deactivateTime);
                AssignDefaults(config, dc.clip.name);

                AssetDatabase.CreateAsset(config, path);
                Debug.Log($"[AttackConfigGen] Created: {assetName}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AttackConfigGen] All AttackConfigs generated in {outputFolder}");
        }

        private string ClipToPatternName(string clipName)
        {
            return clipName switch
            {
                "player_attack1" => "FastJab",
                "player_attack2" => "CrossStrike",
                "player_attack3" => "HeavyStrike",
                "player_air_attack" => "AerialStrike",
                _ => clipName
            };
        }

        private void AssignDefaults(AttackConfig config, string clipName)
        {
            Vector2 size, offset;
            float dmg;
            Vector2 knock;

            switch (clipName)
            {
                case "player_attack1":
                    size = new Vector2(0.8f, 0.5f);
                    offset = new Vector2(0.7f, 0f);
                    dmg = 10f;
                    knock = new Vector2(2f, 0f);
                    break;
                case "player_attack2":
                    size = new Vector2(1.0f, 0.6f);
                    offset = new Vector2(0.8f, 0f);
                    dmg = 12f;
                    knock = new Vector2(3f, 0f);
                    break;
                case "player_attack3":
                    size = new Vector2(1.3f, 0.8f);
                    offset = new Vector2(0.9f, 0f);
                    dmg = 15f;
                    knock = new Vector2(5f, 0f);
                    break;
                case "player_air_attack":
                    size = new Vector2(0.9f, 0.5f);
                    offset = new Vector2(0.6f, -0.2f);
                    dmg = 10f;
                    knock = new Vector2(2f, 0f);
                    break;
                default:
                    size = Vector2.one;
                    offset = Vector2.zero;
                    dmg = 10f;
                    knock = Vector2.zero;
                    break;
            }

            config.SetHitboxShape(false, size, offset);
            config.SetDamage(dmg, knock);
        }

        private void DrawStatus()
        {
            EnsureOutputFolder();
            GUILayout.Label("Status:", EditorStyles.boldLabel);

            var names = new[] { "FastJab", "CrossStrike", "HeavyStrike", "AerialStrike" };
            foreach (var n in names)
            {
                var path = $"{outputFolder}/{n}.asset";
                var exists = AssetDatabase.LoadAssetAtPath<AttackConfig>(path) != null;
                GUILayout.Label($"  {(exists ? "\u2714" : "\u2718")} {n}");
            }
        }

        private void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                var parent = Path.GetDirectoryName(outputFolder).Replace("\\", "/");
                var name = Path.GetFileName(outputFolder);
                if (AssetDatabase.IsValidFolder(parent))
                {
                    var dir = new DirectoryInfo(outputFolder);
                    if (!dir.Exists)
                        AssetDatabase.CreateFolder(parent, name);
                }
            }
        }
    }
}
