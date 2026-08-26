using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Roguelite.Core;
using Roguelite.Player;
using Roguelite.Enemy;
using Roguelite.RoomSystem;
using Roguelite.SaveSystem;
using Roguelite.Combat;

namespace Roguelite.Core
{
    /// <summary>
    /// Công cụ Debug & Cheat Runtime (IMGUI) phục vụ Playtest toàn diện trên bản Build thực tế (Standalone EXE/APK).
    /// Bật/Tắt giao diện bằng phím ~ (BackQuote) hoặc F1.
    /// Giao diện Dark-Neon Cyberpunk hiện đại, trực quan và tối ưu cho Developer/Tester.
    /// </summary>
    public class RuntimeDebugConsole : MonoBehaviour
    {
        public static RuntimeDebugConsole Instance { get; private set; }

        [Header("===== Console Settings =====")]
        [Tooltip("Phím tắt chính để ẩn/hiện bảng Debug.")]
        [SerializeField] private KeyCode toggleKeyPrimary = KeyCode.BackQuote; // Phím ~
        [SerializeField] private KeyCode toggleKeySecondary = KeyCode.F1;

        [Tooltip("Tự động tạo Instance khi Scene khởi chạy nếu chưa có.")]
        [SerializeField] private bool autoCreateOnStart = true;

        // Trạng thái hiển thị cửa sổ IMGUI
        private bool showConsole = false;

        // Khung vị trí cửa sổ (có thể di chuyển được)
        private Rect windowRect = new Rect(15, 15, 840, 580);
        private int activeTab = 0;
        private readonly string[] tabNames = {
            "📜 Logs",
            "⚡ Cheats & Time",
            "👑 Boss Sandbox",
            "👤 Player Stats",
            "⚔️ Weapon Shop",
            "🎁 Perks",
            "💾 Save Inspector",
            "⚙️ Performance"
        };

        private Vector2 perkScrollPos;
        private Vector2 weaponScrollPos;
        private Vector2 saveScrollPos;
        private Vector2 playerStatsScrollPos;

        // Perk Pool Search & Filter Data
        private string perkSearchQuery = "";
        private int perkRarityFilter = 0;
        private readonly string[] perkRarityNames = { "Tất Cả", "Common", "Rare", "Epic", "Legendary" };

        // === LOG CONSOLE DATA ===
        public struct LogEntry
        {
            public string message;
            public string stackTrace;
            public LogType type;
            public string timestamp;
        }

        private List<LogEntry> logEntries = new List<LogEntry>();
        private Vector2 logScrollPos;
        private bool showInfoLogs = true;
        private bool showWarningLogs = true;
        private bool showErrorLogs = true;
        private bool autoScrollLogs = true;
        private const int MAX_LOG_ENTRIES = 200;

        // === CHEAT COMMAND DATA ===
        private string commandInput = "";
        private string commandOutput = "Gõ 'help' để xem danh sách lệnh.";

        // Trạng thái Cheat
        private float currentSpeedMultiplier = 1.0f;
        private float currentDamageMultiplier = 1.0f;
        public float CurrentDamageMultiplier => currentDamageMultiplier;
        private HashSet<string> editingWeaponFoldouts = new HashSet<string>();

        // TimeScale
        private float currentTimeScale = 1.0f;

        // === PERFORMANCE STATS DATA ===
        private float deltaTime = 0.0f;
        private float fps = 0.0f;

        // === CUSTOM GUI STYLES (DARK NEON THEME) ===
        private bool stylesInitialized = false;
        private GUIStyle windowStyle;
        private GUIStyle tabActiveStyle;
        private GUIStyle tabInactiveStyle;
        private GUIStyle closeBtnStyle;
        private GUIStyle cardBoxStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle btnNormalStyle;
        private GUIStyle btnPrimaryStyle;
        private GUIStyle btnDangerStyle;
        private GUIStyle btnWarningStyle;
        private GUIStyle btnToggleOnStyle;
        private GUIStyle btnToggleOffStyle;
        private GUIStyle inputFieldStyle;
        private GUIStyle logBoxStyle;

        private Texture2D texWindowBg;
        private Texture2D texCardBg;
        private Texture2D texTabActive;
        private Texture2D texTabInactive;
        private Texture2D texTabHover;
        private Texture2D texBtnNormal;
        private Texture2D texBtnHover;
        private Texture2D texBtnPrimary;
        private Texture2D texBtnPrimaryHover;
        private Texture2D texBtnDanger;
        private Texture2D texBtnDangerHover;
        private Texture2D texBtnWarning;
        private Texture2D texBtnWarningHover;
        private Texture2D texBtnClose;
        private Texture2D texBtnCloseHover;
        private Texture2D texInputBg;
        private Texture2D texLogBg;

        #region ====== UNITY LIFECYCLE ======

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject consoleObj = new GameObject("[RuntimeDebugConsole]");
                consoleObj.AddComponent<RuntimeDebugConsole>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void Update()
        {
            // Phím tắt bật/tắt Debug Window
            if (Input.GetKeyDown(toggleKeyPrimary) || Input.GetKeyDown(toggleKeySecondary))
            {
                showConsole = !showConsole;
            }

            if (showConsole)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Tính FPS
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;
        }

        private void OnGUI()
        {
            if (!showConsole) return;

            InitializeStyles();

            // GUI Skin & Layout
            GUI.depth = -1000; // Hiển thị trên cùng
            windowRect = GUI.Window(99999, windowRect, DrawConsoleWindow, "<color=#00e5ff><b>🛠️ IN-GAME ADVANCED DEBUG TOOL</b></color> <color=#888888>(2D Roughlite Sandbox)</color>", windowStyle);
        }

        #endregion

        #region ====== CUSTOM THEME INITIALIZATION ======

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            // Generate textures
            texWindowBg = MakeTex(2, 2, new Color(0.08f, 0.10f, 0.14f, 0.97f)); // Dark Navy Slate
            texCardBg = MakeTex(2, 2, new Color(0.12f, 0.15f, 0.21f, 0.92f));   // Card Panel
            texTabActive = MakeTex(2, 2, new Color(0.12f, 0.50f, 0.85f, 0.95f)); // Vibrant Cyan Blue
            texTabInactive = MakeTex(2, 2, new Color(0.16f, 0.19f, 0.26f, 0.85f)); // Dark Inactive
            texTabHover = MakeTex(2, 2, new Color(0.22f, 0.27f, 0.36f, 0.95f));

            texBtnNormal = MakeTex(2, 2, new Color(0.18f, 0.22f, 0.30f, 0.90f));
            texBtnHover = MakeTex(2, 2, new Color(0.26f, 0.32f, 0.44f, 1f));

            texBtnPrimary = MakeTex(2, 2, new Color(0.12f, 0.58f, 0.38f, 0.90f)); // Emerald Green
            texBtnPrimaryHover = MakeTex(2, 2, new Color(0.16f, 0.72f, 0.48f, 1f));

            texBtnDanger = MakeTex(2, 2, new Color(0.72f, 0.18f, 0.22f, 0.90f)); // Crimson Red
            texBtnDangerHover = MakeTex(2, 2, new Color(0.88f, 0.24f, 0.28f, 1f));

            texBtnWarning = MakeTex(2, 2, new Color(0.82f, 0.50f, 0.12f, 0.90f)); // Amber Gold
            texBtnWarningHover = MakeTex(2, 2, new Color(0.95f, 0.60f, 0.16f, 1f));

            texBtnClose = MakeTex(2, 2, new Color(0.80f, 0.18f, 0.22f, 0.95f));
            texBtnCloseHover = MakeTex(2, 2, new Color(0.98f, 0.22f, 0.26f, 1f));

            texInputBg = MakeTex(2, 2, new Color(0.06f, 0.08f, 0.11f, 0.95f));
            texLogBg = MakeTex(2, 2, new Color(0.05f, 0.06f, 0.08f, 0.95f));

            // Window Style
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = texWindowBg;
            windowStyle.onNormal.background = texWindowBg;
            windowStyle.border = new RectOffset(4, 4, 4, 4);
            windowStyle.padding = new RectOffset(10, 10, 26, 10);
            windowStyle.normal.textColor = new Color(0.95f, 0.96f, 0.98f);
            windowStyle.fontSize = 12;
            windowStyle.fontStyle = FontStyle.Bold;
            windowStyle.richText = true;

            // Tab Styles
            tabActiveStyle = new GUIStyle(GUI.skin.button);
            tabActiveStyle.normal.background = texTabActive;
            tabActiveStyle.hover.background = texTabActive;
            tabActiveStyle.normal.textColor = Color.white;
            tabActiveStyle.fontSize = 11;
            tabActiveStyle.fontStyle = FontStyle.Bold;
            tabActiveStyle.margin = new RectOffset(1, 1, 1, 1);

            tabInactiveStyle = new GUIStyle(GUI.skin.button);
            tabInactiveStyle.normal.background = texTabInactive;
            tabInactiveStyle.hover.background = texTabHover;
            tabInactiveStyle.normal.textColor = new Color(0.70f, 0.74f, 0.82f);
            tabInactiveStyle.fontSize = 11;
            tabInactiveStyle.fontStyle = FontStyle.Normal;
            tabInactiveStyle.margin = new RectOffset(1, 1, 1, 1);

            closeBtnStyle = new GUIStyle(GUI.skin.button);
            closeBtnStyle.normal.background = texBtnClose;
            closeBtnStyle.hover.background = texBtnCloseHover;
            closeBtnStyle.normal.textColor = Color.white;
            closeBtnStyle.fontSize = 11;
            closeBtnStyle.fontStyle = FontStyle.Bold;
            closeBtnStyle.margin = new RectOffset(2, 0, 1, 1);

            // Card Style
            cardBoxStyle = new GUIStyle();
            cardBoxStyle.normal.background = texCardBg;
            cardBoxStyle.padding = new RectOffset(10, 10, 8, 8);
            cardBoxStyle.margin = new RectOffset(0, 0, 3, 6);

            cardTitleStyle = new GUIStyle(GUI.skin.label);
            cardTitleStyle.fontSize = 11;
            cardTitleStyle.fontStyle = FontStyle.Bold;
            cardTitleStyle.normal.textColor = new Color(0.00f, 0.88f, 1.0f); // Neon Cyan
            cardTitleStyle.richText = true;

            // Button Styles
            btnNormalStyle = new GUIStyle(GUI.skin.button);
            btnNormalStyle.normal.background = texBtnNormal;
            btnNormalStyle.hover.background = texBtnHover;
            btnNormalStyle.normal.textColor = Color.white;
            btnNormalStyle.fontSize = 10;
            btnNormalStyle.fontStyle = FontStyle.Normal;
            btnNormalStyle.margin = new RectOffset(1, 1, 1, 1);

            btnPrimaryStyle = new GUIStyle(btnNormalStyle);
            btnPrimaryStyle.normal.background = texBtnPrimary;
            btnPrimaryStyle.hover.background = texBtnPrimaryHover;
            btnPrimaryStyle.fontStyle = FontStyle.Bold;

            btnDangerStyle = new GUIStyle(btnNormalStyle);
            btnDangerStyle.normal.background = texBtnDanger;
            btnDangerStyle.hover.background = texBtnDangerHover;
            btnDangerStyle.fontStyle = FontStyle.Bold;

            btnWarningStyle = new GUIStyle(btnNormalStyle);
            btnWarningStyle.normal.background = texBtnWarning;
            btnWarningStyle.hover.background = texBtnWarningHover;
            btnWarningStyle.fontStyle = FontStyle.Bold;

            btnToggleOnStyle = new GUIStyle(btnNormalStyle);
            btnToggleOnStyle.normal.background = texBtnPrimary;
            btnToggleOnStyle.hover.background = texBtnPrimaryHover;
            btnToggleOnStyle.fontStyle = FontStyle.Bold;
            btnToggleOnStyle.normal.textColor = Color.white;

            btnToggleOffStyle = new GUIStyle(btnNormalStyle);
            btnToggleOffStyle.normal.background = texBtnNormal;
            btnToggleOffStyle.hover.background = texBtnHover;
            btnToggleOffStyle.fontStyle = FontStyle.Normal;
            btnToggleOffStyle.normal.textColor = new Color(0.70f, 0.73f, 0.80f);

            // Input Field
            inputFieldStyle = new GUIStyle(GUI.skin.textField);
            inputFieldStyle.normal.background = texInputBg;
            inputFieldStyle.focused.background = texInputBg;
            inputFieldStyle.normal.textColor = new Color(0.35f, 0.95f, 1.0f);
            inputFieldStyle.fontSize = 11;
            inputFieldStyle.padding = new RectOffset(6, 6, 4, 4);

            // Log Container Box
            logBoxStyle = new GUIStyle();
            logBoxStyle.normal.background = texLogBg;
            logBoxStyle.padding = new RectOffset(6, 6, 6, 6);

            stylesInitialized = true;
        }

        #endregion

        #region ====== DRAW WINDOW & TABS ======

        private void DrawConsoleWindow(int windowID)
        {
            // Thanh Tab Bar Tùy Chỉnh
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabNames.Length; i++)
            {
                GUIStyle currentTabStyle = (activeTab == i) ? tabActiveStyle : tabInactiveStyle;
                if (GUILayout.Button(tabNames[i], currentTabStyle, GUILayout.Height(30)))
                {
                    activeTab = i;
                }
            }

            if (GUILayout.Button("✕ Đóng", closeBtnStyle, GUILayout.Width(65), GUILayout.Height(30)))
            {
                showConsole = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Nội dung theo Tab
            switch (activeTab)
            {
                case 0: DrawLogsTab(); break;
                case 1: DrawCheatsTab(); break;
                case 2: DrawBossSandboxTab(); break;
                case 3: DrawPlayerStatsEditorTab(); break;
                case 4: DrawWeaponShopTab(); break;
                case 5: DrawPerksTab(); break;
                case 6: DrawSaveInspectorTab(); break;
                case 7: DrawPerformanceTab(); break;
            }

            // Lệnh kéo rê cửa sổ khi nhấp vào thanh tiêu đề
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 26));
        }

        #endregion

        #region ====== TAB 0: LOG CONSOLE ======

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            LogEntry entry = new LogEntry
            {
                message = logString,
                stackTrace = stackTrace,
                type = type,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            };

            logEntries.Add(entry);
            if (logEntries.Count > MAX_LOG_ENTRIES)
            {
                logEntries.RemoveAt(0);
            }

            if (autoScrollLogs)
            {
                logScrollPos.y = float.MaxValue;
            }
        }

        private void DrawLogsTab()
        {
            GUILayout.BeginVertical(cardBoxStyle);
            
            // Thanh điều khiển Lọc & Xóa Log
            GUILayout.BeginHorizontal();
            showInfoLogs = GUILayout.Toggle(showInfoLogs, " Info", GUILayout.Width(65));
            showWarningLogs = GUILayout.Toggle(showWarningLogs, " Warning", GUILayout.Width(85));
            showErrorLogs = GUILayout.Toggle(showErrorLogs, " Error", GUILayout.Width(75));
            autoScrollLogs = GUILayout.Toggle(autoScrollLogs, " Auto-Scroll", GUILayout.Width(95));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🗑️ Xóa Log", btnNormalStyle, GUILayout.Width(80), GUILayout.Height(22)))
            {
                logEntries.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(3);

            // Bảng danh sách Log
            GUILayout.BeginVertical(logBoxStyle);
            logScrollPos = GUILayout.BeginScrollView(logScrollPos, GUILayout.Height(400));
            
            foreach (var log in logEntries)
            {
                if (log.type == LogType.Log && !showInfoLogs) continue;
                if (log.type == LogType.Warning && !showWarningLogs) continue;
                if ((log.type == LogType.Error || log.type == LogType.Exception) && !showErrorLogs) continue;

                string colorHex = "#ffffff";
                string typePrefix = "[INFO]";

                switch (log.type)
                {
                    case LogType.Warning: colorHex = "#ffcc00"; typePrefix = "[WARN]"; break;
                    case LogType.Error:
                    case LogType.Exception: colorHex = "#ff4d4d"; typePrefix = "[ERR!]"; break;
                    default: colorHex = "#e0e0e0"; typePrefix = "[LOG]"; break;
                }

                GUILayout.Label($"<color=#777777>[{log.timestamp}]</color> <color={colorHex}><b>{typePrefix}</b> {log.message}</color>");
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        #endregion

        #region ====== TAB 1: CHEATS & TIME SCALE ======

        private void DrawCheatsTab()
        {
            // --- SECTION 1: TIME SCALE & SLOW-MOTION ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label($"⏱️ <b>ĐIỀU KHIỂN THỜI GIAN & SLOW-MOTION (Current: <color=#00e5ff>{Time.timeScale:F2}x</color>)</b>", cardTitleStyle);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("⏸️ Pause (x0)", Time.timeScale == 0f ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26))) SetTimeScale(0f);
            if (GUILayout.Button("▶️ x0.1 (Slow-Mo)", Time.timeScale == 0.1f ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26))) SetTimeScale(0.1f);
            if (GUILayout.Button("▶️ x0.25", Time.timeScale == 0.25f ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26))) SetTimeScale(0.25f);
            if (GUILayout.Button("▶️ x0.5", Time.timeScale == 0.5f ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26))) SetTimeScale(0.5f);
            if (GUILayout.Button("▶️ x1.0 (Normal)", Time.timeScale == 1.0f ? btnToggleOnStyle : btnPrimaryStyle, GUILayout.Height(26))) SetTimeScale(1.0f);
            if (GUILayout.Button("⏩ x2.0", Time.timeScale == 2.0f ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26))) SetTimeScale(2.0f);
            if (GUILayout.Button("⏩ x5.0", Time.timeScale == 5.0f ? btnToggleOnStyle : btnWarningStyle, GUILayout.Height(26))) SetTimeScale(5.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Slider Tốc Độ:", GUILayout.Width(90));
            float newScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 3f);
            if (Mathf.Abs(newScale - Time.timeScale) > 0.01f)
            {
                SetTimeScale(newScale);
            }
            GUILayout.Label($"{Time.timeScale:F2}x", GUILayout.Width(45));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(4);

            // --- SECTION 2: QUICK CHEATS ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("⚡ <b>THAO TÁC NHANH (QUICK ACTIONS)</b>", cardTitleStyle);
            GUILayout.Space(4);

            PlayerStats playerStats = FindObjectOfType<PlayerStats>();

            GUILayout.BeginHorizontal();
            bool currentGodMode = playerStats != null && playerStats.IsGodMode;
            GUIStyle godStyle = currentGodMode ? btnToggleOnStyle : btnToggleOffStyle;
            if (GUILayout.Button(currentGodMode ? "🛡️ God Mode: ON" : "🛡️ God Mode: OFF", godStyle, GUILayout.Height(30)))
            {
                if (playerStats != null)
                {
                    playerStats.IsGodMode = !playerStats.IsGodMode;
                    commandOutput = $"🛡️ GodMode: {(playerStats.IsGodMode ? "BẬT" : "TẮT")}";
                }
            }

            if (GUILayout.Button("❤️ Hồi Đầy Máu", btnPrimaryStyle, GUILayout.Height(30)))
            {
                if (playerStats != null)
                {
                    playerStats.Heal(99999f);
                    commandOutput = "❤️ Đã hồi đầy máu Player!";
                }
            }

            if (GUILayout.Button("💰 +1,000 Vàng", btnWarningStyle, GUILayout.Height(30)))
            {
                AddGold(1000);
            }

            if (GUILayout.Button("☠️ Diệt Quái Phòng", btnDangerStyle, GUILayout.Height(30)))
            {
                KillEnemiesInCurrentRoomOnly();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔓 Mở Cửa Phòng Này", btnNormalStyle, GUILayout.Height(28))) ClearCurrentRoomDoors();
            if (GUILayout.Button("🚪 Skip Sang Phòng Tiếp", btnNormalStyle, GUILayout.Height(28))) SkipToNextRoom();
            if (GUILayout.Button("👑 Skip Tới Phòng Boss", btnWarningStyle, GUILayout.Height(28))) SkipToBossRoom();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(4);

            // --- SECTION 3: COMMAND CONSOLE ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("💻 <b>KHUNG NHẬP LỆNH (COMMAND CONSOLE)</b>", cardTitleStyle);
            GUILayout.Space(4);
            
            GUILayout.BeginHorizontal();
            commandInput = GUILayout.TextField(commandInput, inputFieldStyle, GUILayout.Height(26));
            if (GUILayout.Button("Gửi Lệnh", btnPrimaryStyle, GUILayout.Width(90), GUILayout.Height(26)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                ExecuteCommand(commandInput);
                commandInput = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUILayout.Label($"<color=#00e5ff>{commandOutput}</color>");
            GUILayout.EndVertical();
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
            commandOutput = $"⏱️ Đã đặt TimeScale = {Time.timeScale:F2}x";
        }

        #endregion

        #region ====== TAB 2: BOSS & COMBAT SANDBOX ======

        private void DrawBossSandboxTab()
        {
            WorldBoss worldBoss = FindObjectOfType<WorldBoss>();
            BossBase genericBoss = FindObjectOfType<BossBase>();

            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("👑 <b>QUẢN LÝ & ĐIỀU KHIỂN BOSS SANDBOX</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (worldBoss != null)
            {
                float bossHpPercent = worldBoss.MaxHP > 0 ? (worldBoss.CurrentHP / worldBoss.MaxHP) * 100f : 0f;
                GUILayout.Label($"• <b>Target Boss:</b> <color=#00ff88><b>{worldBoss.gameObject.name}</b></color> (WorldBoss v{WorldBoss.VERSION})");
                GUILayout.Label($"• <b>Máu Boss:</b> <color=#ff4d4d>{worldBoss.CurrentHP:F1}</color> / {worldBoss.MaxHP:F1} ({bossHpPercent:F0}%) | <b>Phase:</b> <color=#00e5ff>{worldBoss.CurrentPhase}</color>/{worldBoss.TotalPhases - 1}");
                GUILayout.Label($"• <b>Tích Lũy Đòn Đánh:</b> [{worldBoss.CurrentAttackCount}/{worldBoss.AttacksBeforeMeteor}] | <b>Trạng thái:</b> {(worldBoss.IsCastingMeteor ? "<color=#ffcc00>ĐANG TỤ METEOR</color>" : worldBoss.CurrentState.ToString())} | <b>Grounded:</b> {worldBoss.IsGrounded}");
                GUILayout.Label($"• <b>Dummy Mode (Bất Tử / Khóa Máu):</b> {(worldBoss.IsDummyMode ? "<color=#00ff88>BẬT</color>" : "<color=#ff4d4d>TẮT</color>")}");

                GUILayout.Space(6);

                // --- HÀNG 1: DUMMY MODE & EP METEOR RAIN ---
                GUILayout.BeginHorizontal();
                GUIStyle dummyStyle = worldBoss.IsDummyMode ? btnToggleOnStyle : btnToggleOffStyle;
                if (GUILayout.Button(worldBoss.IsDummyMode ? "🛡️ Dummy Mode: ON" : "🛡️ Dummy Mode: OFF", dummyStyle, GUILayout.Height(30)))
                {
                    worldBoss.IsDummyMode = !worldBoss.IsDummyMode;
                    commandOutput = $"🛡️ Boss Dummy Mode: {(worldBoss.IsDummyMode ? "BẬT" : "TẮT")}";
                }

                if (GUILayout.Button("🔥 Ép Cast Mưa Thiên Thạch", btnWarningStyle, GUILayout.Height(30)))
                {
                    worldBoss.ForceTriggerMeteorRain();
                    commandOutput = "🔥 Đã ép Boss kích hoạt chiêu Mưa Thiên Thạch!";
                }

                if (GUILayout.Button("❤️ Hồi Đầy Máu Boss", btnPrimaryStyle, GUILayout.Height(30)))
                {
                    worldBoss.ResetBossHealth();
                    commandOutput = "❤️ Đã hồi đầy máu Boss!";
                }

                if (GUILayout.Button("☠️ Diệt Boss Ngay", btnDangerStyle, GUILayout.Height(30)))
                {
                    worldBoss.TakeDamage(999999f, Vector2.zero);
                    commandOutput = "☠️ Đã tiêu diệt Boss!";
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

                // --- HÀNG 2: EP CHUYỂN PHASE ---
                GUILayout.Label("🌟 <b>ÉP CHUYỂN PHASE BOSS TỨC THÌ:</b>", cardTitleStyle);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Phase 0 (100% HP)", btnNormalStyle, GUILayout.Height(26))) worldBoss.ForceSetPhase(0);
                if (GUILayout.Button("Phase 1 (Enrage 1)", btnNormalStyle, GUILayout.Height(26))) worldBoss.ForceSetPhase(1);
                if (GUILayout.Button("Phase 2 (Enrage 2)", btnDangerStyle, GUILayout.Height(26))) worldBoss.ForceSetPhase(2);
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

                // --- HÀNG 3: TELEPORT TƯƠNG TÁC ---
                GUILayout.Label("📍 <b>ĐIỀU HƯỚNG VỊ TRÍ:</b>", cardTitleStyle);
                GUILayout.BeginHorizontal();
                var player = FindObjectOfType<PlayerController>();
                if (GUILayout.Button("🚀 Dịch Chuyển Player Đến Boss", btnNormalStyle, GUILayout.Height(26)))
                {
                    if (player != null)
                    {
                        player.transform.position = worldBoss.transform.position + Vector3.left * 3f;
                        commandOutput = "🚀 Đã dịch chuyển Player tới cạnh Boss!";
                    }
                }
                if (GUILayout.Button("🧲 Kéo Boss Lại Gần Player", btnNormalStyle, GUILayout.Height(26)))
                {
                    if (player != null)
                    {
                        worldBoss.transform.position = player.transform.position + Vector3.right * 4f;
                        commandOutput = "🧲 Đã kéo Boss lại gần Player!";
                    }
                }
                GUILayout.EndHorizontal();
            }
            else if (genericBoss != null)
            {
                GUILayout.Label($"• <b>Target Boss:</b> <color=#00ff88><b>{genericBoss.gameObject.name}</b></color> (BossBase)");
                GUILayout.Label($"• <b>Máu Boss:</b> <color=#ff4d4d>{genericBoss.CurrentHP:F1}</color> / {genericBoss.MaxHP:F1} | <b>Phase:</b> {genericBoss.CurrentPhase}");
                GUILayout.Space(4);
                if (GUILayout.Button("☠️ Diệt Boss Ngay", btnDangerStyle, GUILayout.Height(28)))
                {
                    genericBoss.TakeDamage(999999f, Vector2.zero);
                }
            }
            else
            {
                GUILayout.Label("<color=#ffcc00>⚠️ Hiện tại không có Boss nào trong Scene!</color>");
                GUILayout.Space(6);
                if (GUILayout.Button("👑 Dịch Chuyển Tới Phòng Boss", btnWarningStyle, GUILayout.Height(30)))
                {
                    SkipToBossRoom();
                }
            }

            GUILayout.EndVertical();
        }

        #endregion

        #region ====== TAB 3: PLAYER STATS EDITOR ======

        private void DrawPlayerStatsEditorTab()
        {
            playerStatsScrollPos = GUILayout.BeginScrollView(playerStatsScrollPos, GUILayout.Height(430));

            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            PlayerController playerController = FindObjectOfType<PlayerController>();

            // --- SECTION 1: HEALTH & INVINCIBILITY EDITOR ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("❤️ <b>ĐIỀU CHỈNH MÁU & TRẠNG THÁI SỐNG PLAYER</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (playerStats != null)
            {
                GUILayout.Label($"• <b>Máu Hiện Tại:</b> <color=#00ff88>{playerStats.CurrentHealth:F1}</color> / <b>Máu Tối Đa (Max HP):</b> <color=#00e5ff>{playerStats.MaxHealth:F1}</color>");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Máu Max (Max HP):", GUILayout.Width(130));
                if (GUILayout.Button("-50", btnNormalStyle, GUILayout.Width(45), GUILayout.Height(22))) playerStats.SetMaxHealthDirect(playerStats.MaxHealth - 50);
                if (GUILayout.Button("-10", btnNormalStyle, GUILayout.Width(45), GUILayout.Height(22))) playerStats.SetMaxHealthDirect(playerStats.MaxHealth - 10);
                if (GUILayout.Button("+10", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) playerStats.SetMaxHealthDirect(playerStats.MaxHealth + 10);
                if (GUILayout.Button("+50", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) playerStats.SetMaxHealthDirect(playerStats.MaxHealth + 50);
                if (GUILayout.Button("+200", btnPrimaryStyle, GUILayout.Width(50), GUILayout.Height(22))) playerStats.SetMaxHealthDirect(playerStats.MaxHealth + 200);
                if (GUILayout.Button("Reset 100", btnWarningStyle, GUILayout.Width(75), GUILayout.Height(22))) playerStats.SetMaxHealthDirect(100f);
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Máu Hiện Tại (HP):", GUILayout.Width(130));
                if (GUILayout.Button("1 HP", btnDangerStyle, GUILayout.Width(50), GUILayout.Height(22))) playerStats.SetCurrentHealthDirect(1f);
                if (GUILayout.Button("50%", btnWarningStyle, GUILayout.Width(50), GUILayout.Height(22))) playerStats.SetCurrentHealthDirect(playerStats.MaxHealth * 0.5f);
                if (GUILayout.Button("100% Full", btnPrimaryStyle, GUILayout.Width(75), GUILayout.Height(22))) playerStats.SetCurrentHealthDirect(playerStats.MaxHealth);
                if (GUILayout.Button("+20 HP", btnPrimaryStyle, GUILayout.Width(60), GUILayout.Height(22))) playerStats.Heal(20f);
                if (GUILayout.Button("-20 HP", btnDangerStyle, GUILayout.Width(60), GUILayout.Height(22))) playerStats.TakeDamage(20f, Vector2.zero);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("<color=#ffcc00>⚠️ Không tìm thấy PlayerStats trong Scene!</color>");
            }
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // --- SECTION 2: MOVEMENT & DASH SPEED EDITOR ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("🏃 <b>ĐIỀU CHỈNH TỐC ĐỘ DI CHUYỂN & NHẢY</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (playerController != null)
            {
                GUILayout.Label($"• <b>Walk Speed:</b> {playerController.walkSpeed:F1} | <b>Run Speed:</b> {playerController.runSpeed:F1} | <b>Jump Force:</b> {playerController.jumpForce:F1}");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Run Speed:", GUILayout.Width(90));
                if (GUILayout.Button("-2.0", btnNormalStyle, GUILayout.Width(45), GUILayout.Height(22))) playerController.runSpeed = Mathf.Max(1f, playerController.runSpeed - 2f);
                if (GUILayout.Button("+2.0", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) playerController.runSpeed += 2f;
                if (GUILayout.Button("x1.5", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) playerController.runSpeed *= 1.5f;
                if (GUILayout.Button("x2.0", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) playerController.runSpeed *= 2f;
                if (GUILayout.Button("Mặc Định (8.0)", btnWarningStyle, GUILayout.Width(100), GUILayout.Height(22))) playerController.runSpeed = 8f;
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Force:", GUILayout.Width(90));
                if (GUILayout.Button("-2.0", btnNormalStyle, GUILayout.Width(45), GUILayout.Height(22))) playerController.jumpForce = Mathf.Max(1f, playerController.jumpForce - 2f);
                if (GUILayout.Button("+2.0", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) playerController.jumpForce += 2f;
                if (GUILayout.Button("Mặc Định (12.0)", btnWarningStyle, GUILayout.Width(100), GUILayout.Height(22))) playerController.jumpForce = 12f;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // --- SECTION 3: ATTACK DAMAGE & MULTIPLIER ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("🗡️ <b>ĐIỀU CHỈNH SÁT THƯƠNG ĐÒN ĐÁNH PLAYER</b>", cardTitleStyle);
            GUILayout.Space(4);

            Attack playerAttack = playerStats != null ? playerStats.GetComponentInChildren<Attack>(true) : null;
            float baseDam = playerAttack != null ? playerAttack.BaseAttackDamage : 10f;
            float totalDam = playerAttack != null ? playerAttack.GetCalculatedDamage() : baseDam * currentDamageMultiplier;

            GUILayout.Label($"• <b>Sát Thương Gốc (Base):</b> <color=#ff4d4d>{baseDam:F1}</color> | <b>Damage Multiplier:</b> <color=#00e5ff>x{currentDamageMultiplier:F1}</color> → <b>Tổng Gây Ra:</b> <color=#00ff88><b>{totalDam:F1}</b></color>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Damage x1.0 (Reset)", btnNormalStyle, GUILayout.Height(24))) SetDamageMultiplier(1.0f);
            if (GUILayout.Button("Damage x1.5", btnNormalStyle, GUILayout.Height(24))) SetDamageMultiplier(1.5f);
            if (GUILayout.Button("Damage x2.0", btnPrimaryStyle, GUILayout.Height(24))) SetDamageMultiplier(2.0f);
            if (GUILayout.Button("Damage x5.0 (One Shot)", btnDangerStyle, GUILayout.Height(24))) SetDamageMultiplier(5.0f);
            if (GUILayout.Button("Damage x10.0", btnDangerStyle, GUILayout.Height(24))) SetDamageMultiplier(10.0f);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        #endregion

        #region ====== TAB 4: WEAPON SHOP ======

        private void DrawWeaponShopTab()
        {
            weaponScrollPos = GUILayout.BeginScrollView(weaponScrollPos, GUILayout.Height(430));

            var progress = SaveManager.Instance?.CurrentSaveData?.progressData;
            int gold = progress != null ? progress.totalCurrency : 0;
            int kills = progress != null ? progress.totalEnemiesKilled : 0;
            int runs = progress != null ? progress.totalRunsPlayed : 0;
            int rooms = progress != null ? progress.highestRoomReached : 0;

            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("⚔️ <b>QUẢN LÝ WEAPON SHOP & ĐIỀU KIỆN MỞ KHÓA</b>", cardTitleStyle);
            GUILayout.Space(4);

            GUILayout.Label($"• <b>Vàng (Gold):</b> <color=#ffcc00>{gold}</color> | <b>Quái đã diệt:</b> <color=#00e5ff>{kills}</color> | <b>Lượt Run:</b> <color=#00ff88>{runs}</color> | <b>Phòng sâu nhất:</b> {rooms}");
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔓 Mở Khóa Tất Cả Vũ Khí", btnPrimaryStyle, GUILayout.Height(26)))
            {
                WeaponShopManager.Instance?.UnlockAllWeapons();
                commandOutput = "🔓 Đã mở khóa toàn bộ vũ khí!";
            }
            if (GUILayout.Button("🔒 Reset Mở Khóa Vũ Khí", btnDangerStyle, GUILayout.Height(26)))
            {
                WeaponShopManager.Instance?.ResetWeaponUnlocks();
                commandOutput = "🔒 Đã reset mở khóa vũ khí!";
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // SECTION: EQUIPPED WEAPONS
            GUILayout.BeginVertical(cardBoxStyle);
            WeaponDatabase db = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.Database : WeaponShopManager.GetOrLoadWeaponDatabase();
            var equippedIds = SaveManager.Instance?.CurrentSaveData?.weaponData?.equippedWeaponIds;
            int equippedCount = equippedIds != null ? equippedIds.Count : 0;

            GUILayout.Label($"🎯 <b>WEAPONS BUS SUPPORT ĐANG TRANG BỊ (<color=#00ff88>{equippedCount}/{WeaponUnlockData.MAX_EQUIPPED_SLOTS}</color> Slots)</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (equippedIds != null && equippedIds.Count > 0)
            {
                foreach (string id in equippedIds)
                {
                    WeaponData w = db != null ? db.GetWeaponById(id) : null;
                    if (w == null) continue;

                    GUILayout.BeginHorizontal(logBoxStyle);
                    GUILayout.Label($"🗡️ <b>{w.WeaponName}</b> <color=#888888>({id})</color> | Dam: <color=#ff4d4d>+{w.Damage}</color> | Knockback: <color=#00e5ff>({w.Knockback.x:F1}, {w.Knockback.y:F1})</color> | Spd: {w.AttackSpeed}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("❌ Gỡ", btnDangerStyle, GUILayout.Width(70), GUILayout.Height(20)))
                    {
                        WeaponShopManager.Instance?.UnequipSupportWeapon(w);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("<color=#ffcc00>⚠️ Chưa trang bị vũ khí support nào.</color>");
            }
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // SECTION: WEAPONS LIST
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("📜 <b>DANH SÁCH VŨ KHÍ TRONG DATABASE</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (db != null && db.AllWeapons != null)
            {
                foreach (WeaponData weapon in db.AllWeapons)
                {
                    if (weapon == null) continue;
                    string weaponId = WeaponShopManager.Instance != null ? WeaponShopManager.Instance.GetWeaponId(weapon) : weapon.WeaponId;
                    bool isUnlocked = WeaponShopManager.Instance != null && WeaponShopManager.Instance.IsWeaponUnlocked(weaponId);
                    bool isEquipped = WeaponShopManager.Instance != null && WeaponShopManager.Instance.IsWeaponEquipped(weapon);

                    GUILayout.BeginVertical(logBoxStyle);
                    GUILayout.BeginHorizontal();
                    string tag = isEquipped ? "<color=#00ff88>[ĐÃ TRANG BỊ]</color>" : (isUnlocked ? "<color=#00e5ff>[ĐÃ MỞ KHÓA]</color>" : "<color=#ff4d4d>[KHÓA]</color>");
                    GUILayout.Label($"<b>{weapon.WeaponName}</b> {tag} | Giá: <color=#ffcc00>{weapon.Price}G</color>");
                    GUILayout.FlexibleSpace();

                    if (!isUnlocked)
                    {
                        if (GUILayout.Button("🔓 Mở Khóa Free", btnNormalStyle, GUILayout.Width(100), GUILayout.Height(22)))
                        {
                            var wData = SaveManager.Instance?.CurrentSaveData?.weaponData;
                            if (wData != null && !wData.unlockedWeaponIds.Contains(weaponId))
                            {
                                wData.unlockedWeaponIds.Add(weaponId);
                                SaveManager.Instance.TriggerAutoSave(0.2f);
                            }
                        }
                    }
                    else
                    {
                        if (isEquipped)
                        {
                            if (GUILayout.Button("Gỡ Support", btnDangerStyle, GUILayout.Width(90), GUILayout.Height(22)))
                            {
                                WeaponShopManager.Instance?.UnequipSupportWeapon(weapon);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("+ Trang Bị", btnPrimaryStyle, GUILayout.Width(90), GUILayout.Height(22)))
                            {
                                WeaponShopManager.Instance?.EquipSupportWeapon(weapon);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        #endregion

        #region ====== TAB 5: PERKS & BUFFS ======

        private void DrawPerksTab()
        {
            perkScrollPos = GUILayout.BeginScrollView(perkScrollPos, GUILayout.Height(430));

            var um = Roguelite.UpgradeSystem.UpgradeManager.Instance;

            // =========================================================================
            //  SECTION 1: ACTIVE PERKS TRÊN PLAYER (TỔNG QUAN & QUẢN LÝ NHANH)
            // =========================================================================
            GUILayout.BeginVertical(cardBoxStyle);
            int activeCount = um != null && um.ActivePerks != null ? um.ActivePerks.Count : 0;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"✨ <b>DANH SÁCH PERK ĐANG ACTIVE TRÊN PLAYER (<color=#00ff88>{activeCount}</color> Perks)</b>", cardTitleStyle);
            GUILayout.FlexibleSpace();
            if (activeCount > 0)
            {
                if (GUILayout.Button("🧹 Xóa Toàn Bộ Perk Đang Có", btnDangerStyle, GUILayout.Width(170), GUILayout.Height(22)))
                {
                    um?.ClearAllActivePerks();
                    commandOutput = "🧹 Đã xóa toàn bộ Perk đang active trên Player!";
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            if (um != null && um.ActivePerks != null && um.ActivePerks.Count > 0)
            {
                foreach (var pair in new Dictionary<Roguelite.UpgradeSystem.PerkData, int>(um.ActivePerks))
                {
                    var perk = pair.Key;
                    int stack = pair.Value;
                    if (perk == null) continue;

                    string rarityColor = GetRarityColorHex(perk.Rarity);

                    GUILayout.BeginHorizontal(logBoxStyle);
                    GUILayout.Label($"• <color={rarityColor}><b>[{perk.Rarity}]</b></color> <b>{perk.PerkName}</b> <color=#888888>(Stack: <color=#00ff88>{stack}/{perk.MaxStack}</color>)</color> - {perk.Description}");
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+1", btnPrimaryStyle, GUILayout.Width(35), GUILayout.Height(20)))
                    {
                        um.AddPerk(perk);
                    }
                    if (GUILayout.Button("-1", btnWarningStyle, GUILayout.Width(35), GUILayout.Height(20)))
                    {
                        um.RemovePerk(perk);
                    }
                    if (GUILayout.Button("Max", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(20)))
                    {
                        while (um.ActivePerks.ContainsKey(perk) && um.ActivePerks[perk] < perk.MaxStack)
                        {
                            um.AddPerk(perk);
                        }
                    }
                    if (GUILayout.Button("❌ Xóa", btnDangerStyle, GUILayout.Width(55), GUILayout.Height(20)))
                    {
                        while (um.ActivePerks.ContainsKey(perk))
                        {
                            um.RemovePerk(perk);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }
            else
            {
                GUILayout.Label("<color=#888888>Chưa có Perk nào đang active trên người Player. Hãy chọn bất kỳ Perk nào từ Bể Perk bên dưới để thêm ngay lập tức!</color>");
            }
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // =========================================================================
            //  SECTION 2: BỂ CHỨA PERK (PERK POOL BROWSER - TỰ DO CHỌN 100%)
            // =========================================================================
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("📚 <b>BỂ CHỨA TOÀN BỘ PERK (PERK POOL BROWSER - TỰ DO THÊM BẤT KỲ PERK)</b>", cardTitleStyle);
            GUILayout.Space(4);

            // Thanh Tìm Kiếm & Bộ Lọc
            GUILayout.BeginHorizontal();
            GUILayout.Label("🔍 Tìm:", GUILayout.Width(45));
            perkSearchQuery = GUILayout.TextField(perkSearchQuery, inputFieldStyle, GUILayout.Width(180), GUILayout.Height(24));
            if (!string.IsNullOrEmpty(perkSearchQuery))
            {
                if (GUILayout.Button("✕", btnNormalStyle, GUILayout.Width(25), GUILayout.Height(24)))
                {
                    perkSearchQuery = "";
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Lọc Rarity:", GUILayout.Width(70));
            for (int i = 0; i < perkRarityNames.Length; i++)
            {
                GUIStyle filterBtnStyle = (perkRarityFilter == i) ? btnToggleOnStyle : btnNormalStyle;
                if (GUILayout.Button(perkRarityNames[i], filterBtnStyle, GUILayout.Height(24)))
                {
                    perkRarityFilter = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Danh sách toàn bộ Perk trong game
            var pool = um != null ? um.PerkPool : null;
            if (pool != null && pool.AllPerks != null && pool.AllPerks.Count > 0)
            {
                int matchCount = 0;
                foreach (var perk in pool.AllPerks)
                {
                    if (perk == null) continue;

                    // Kiểm tra tìm kiếm
                    if (!string.IsNullOrEmpty(perkSearchQuery))
                    {
                        bool matchName = perk.PerkName.IndexOf(perkSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool matchDesc = !string.IsNullOrEmpty(perk.Description) && perk.Description.IndexOf(perkSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!matchName && !matchDesc) continue;
                    }

                    // Kiểm tra Rarity filter
                    if (perkRarityFilter > 0)
                    {
                        string targetRarity = perkRarityNames[perkRarityFilter];
                        if (perk.Rarity.ToString() != targetRarity) continue;
                    }

                    matchCount++;
                    string rarityColor = GetRarityColorHex(perk.Rarity);
                    int currentStack = (um.ActivePerks != null && um.ActivePerks.ContainsKey(perk)) ? um.ActivePerks[perk] : 0;
                    bool isForced = um.ForcedNextPerk == perk;

                    GUILayout.BeginVertical(logBoxStyle);
                    GUILayout.BeginHorizontal();

                    GUILayout.Label($"<color={rarityColor}><b>[{perk.Rarity}]</b></color> <b>{perk.PerkName}</b> <color=#888888>(Đang có: <color=#00ff88>{currentStack}/{perk.MaxStack}</color>)</color>");
                    GUILayout.FlexibleSpace();

                    // Nút +1 Nhận Trực Tiếp
                    if (GUILayout.Button("⚡ + Nhận 1", btnPrimaryStyle, GUILayout.Width(90), GUILayout.Height(22)))
                    {
                        um.AddPerk(perk);
                        commandOutput = $"⚡ Đã thêm 1 stack Perk '{perk.PerkName}'!";
                    }

                    // Nút Max Stack
                    if (GUILayout.Button("Max Stack", btnPrimaryStyle, GUILayout.Width(80), GUILayout.Height(22)))
                    {
                        while (um.ActivePerks.ContainsKey(perk) ? um.ActivePerks[perk] < perk.MaxStack : true)
                        {
                            um.AddPerk(perk);
                            if (um.ActivePerks[perk] >= perk.MaxStack) break;
                        }
                        commandOutput = $"⚡ Đã nạp đầy Max Stack cho Perk '{perk.PerkName}'!";
                    }

                    // Nút Force Drop
                    GUIStyle forceBtnStyle = isForced ? btnWarningStyle : btnNormalStyle;
                    if (GUILayout.Button(isForced ? "★ Đang Force" : "Force Drop", forceBtnStyle, GUILayout.Width(90), GUILayout.Height(22)))
                    {
                        if (isForced) um.SetForcedNextPerk(null);
                        else um.SetForcedNextPerk(perk);
                        commandOutput = isForced ? $"Đã hủy Force Perk '{perk.PerkName}'" : $"★ Đã ép rơi Perk '{perk.PerkName}' ở lần chọn thưởng tiếp theo!";
                    }

                    // Nút Trừ bớt
                    if (currentStack > 0)
                    {
                        if (GUILayout.Button("-1", btnWarningStyle, GUILayout.Width(35), GUILayout.Height(22)))
                        {
                            um.RemovePerk(perk);
                        }
                    }

                    GUILayout.EndHorizontal();

                    // Mô tả hiệu ứng Perk
                    GUILayout.Label($"<color=#cccccc><i>{perk.Description}</i></color>");

                    GUILayout.EndVertical();
                    GUILayout.Space(3);
                }

                if (matchCount == 0)
                {
                    GUILayout.Label("<color=#ffcc00>⚠️ Không tìm thấy Perk nào khớp với bộ lọc tìm kiếm!</color>");
                }
            }
            else
            {
                GUILayout.Label("<color=#ffcc00>⚠️ Chưa tìm thấy PerkPool trong UpgradeManager!</color>");
            }
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // =========================================================================
            //  SECTION 3: THỬ NGHIỆM TỶ LỆ MỞ THẺ & GIAO DIỆN 3-CARD REWARD
            // =========================================================================
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("🎲 <b>CÂN BẰNG TỶ LỆ RƠI THẺ (RARITY WEIGHTS) & MỞ GIAO DIỆN CHỌN THẺ</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (um != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Common: {um.CommonWeight}", GUILayout.Width(100));
                um.CommonWeight = Mathf.RoundToInt(GUILayout.HorizontalSlider(um.CommonWeight, 0, 200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Rare: {um.RareWeight}", GUILayout.Width(100));
                um.RareWeight = Mathf.RoundToInt(GUILayout.HorizontalSlider(um.RareWeight, 0, 200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Epic: {um.EpicWeight}", GUILayout.Width(100));
                um.EpicWeight = Mathf.RoundToInt(GUILayout.HorizontalSlider(um.EpicWeight, 0, 200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Legendary: {um.LegendaryWeight}", GUILayout.Width(100));
                um.LegendaryWeight = Mathf.RoundToInt(GUILayout.HorizontalSlider(um.LegendaryWeight, 0, 200));
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("🔄 Reset Tỷ Lệ Mặc Định (60/25/12/3)", btnNormalStyle, GUILayout.Height(26)))
                {
                    um.CommonWeight = 60;
                    um.RareWeight = 25;
                    um.EpicWeight = 12;
                    um.LegendaryWeight = 3;
                }

                if (GUILayout.Button("🎁 Mở Bảng Chọn Perk (3 Card Reward Gameplay)", btnPrimaryStyle, GUILayout.Height(26)))
                {
                    var rsc = Roguelite.UI.RewardSelectionController.Instance;
                    if (rsc != null)
                    {
                        rsc.OpenSelection();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        private string GetRarityColorHex(Roguelite.UpgradeSystem.PerkRarity rarity)
        {
            switch (rarity)
            {
                case Roguelite.UpgradeSystem.PerkRarity.Common: return "#cccccc";
                case Roguelite.UpgradeSystem.PerkRarity.Rare: return "#00e5ff";
                case Roguelite.UpgradeSystem.PerkRarity.Epic: return "#b300ff";
                case Roguelite.UpgradeSystem.PerkRarity.Legendary: return "#ffcc00";
                default: return "#ffffff";
            }
        }

        #endregion

        #region ====== TAB 6: SAVE INSPECTOR & SYNC ======

        private void DrawSaveInspectorTab()
        {
            saveScrollPos = GUILayout.BeginScrollView(saveScrollPos, GUILayout.Height(430));

            SaveManager sm = SaveManager.Instance;
            SaveData sd = sm?.CurrentSaveData;
            var pd = sd?.progressData;

            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label($"💾 <b>QUẢN LÝ TIẾN TRÌNH SAVE FILE (SLOT {sm?.CurrentSlotIndex ?? 1})</b>", cardTitleStyle);
            GUILayout.Space(4);

            if (pd != null)
            {
                // 1. SỐ LẦN CHẠY (RUNS PLAYED)
                GUILayout.BeginHorizontal();
                GUILayout.Label($"• <b>Lượt Chạy (Runs Played):</b> <color=#00ff88>{pd.totalRunsPlayed}</color>", GUILayout.Width(220));
                if (GUILayout.Button("-1", btnNormalStyle, GUILayout.Width(40), GUILayout.Height(22))) { pd.totalRunsPlayed = Mathf.Max(0, pd.totalRunsPlayed - 1); }
                if (GUILayout.Button("+1", btnPrimaryStyle, GUILayout.Width(40), GUILayout.Height(22))) { pd.totalRunsPlayed += 1; }
                if (GUILayout.Button("+5", btnPrimaryStyle, GUILayout.Width(40), GUILayout.Height(22))) { pd.totalRunsPlayed += 5; }
                if (GUILayout.Button("+10", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) { pd.totalRunsPlayed += 10; }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                // 2. SỐ QUÁI ĐÃ DIỆT (ENEMIES KILLED)
                GUILayout.BeginHorizontal();
                GUILayout.Label($"• <b>Quái Đã Diệt (Enemies Killed):</b> <color=#00e5ff>{pd.totalEnemiesKilled}</color>", GUILayout.Width(220));
                if (GUILayout.Button("-50", btnNormalStyle, GUILayout.Width(45), GUILayout.Height(22))) { pd.totalEnemiesKilled = Mathf.Max(0, pd.totalEnemiesKilled - 50); }
                if (GUILayout.Button("+50", btnPrimaryStyle, GUILayout.Width(45), GUILayout.Height(22))) { pd.totalEnemiesKilled += 50; }
                if (GUILayout.Button("+100", btnPrimaryStyle, GUILayout.Width(50), GUILayout.Height(22))) { pd.totalEnemiesKilled += 100; }
                if (GUILayout.Button("+500", btnPrimaryStyle, GUILayout.Width(50), GUILayout.Height(22))) { pd.totalEnemiesKilled += 500; }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                // 3. PHÒNG SÂU NHẤT (HIGHEST ROOM)
                GUILayout.BeginHorizontal();
                GUILayout.Label($"• <b>Phòng Sâu Nhất (Room Depth):</b> <color=#ffcc00>{pd.highestRoomReached}</color>", GUILayout.Width(220));
                if (GUILayout.Button("-1", btnNormalStyle, GUILayout.Width(40), GUILayout.Height(22))) { pd.highestRoomReached = Mathf.Max(0, pd.highestRoomReached - 1); }
                if (GUILayout.Button("+1", btnPrimaryStyle, GUILayout.Width(40), GUILayout.Height(22))) { pd.highestRoomReached += 1; }
                if (GUILayout.Button("+5", btnPrimaryStyle, GUILayout.Width(40), GUILayout.Height(22))) { pd.highestRoomReached += 5; }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                // 4. VÀNG / TIỀN TỆ (TOTAL CURRENCY)
                GUILayout.BeginHorizontal();
                GUILayout.Label($"• <b>Vàng (Gold/Currency):</b> <color=#ffcc00>{pd.totalCurrency}</color>", GUILayout.Width(220));
                if (GUILayout.Button("+500", btnWarningStyle, GUILayout.Width(50), GUILayout.Height(22))) { AddGold(500); }
                if (GUILayout.Button("+1,000", btnWarningStyle, GUILayout.Width(60), GUILayout.Height(22))) { AddGold(1000); }
                if (GUILayout.Button("+5,000", btnWarningStyle, GUILayout.Width(60), GUILayout.Height(22))) { AddGold(5000); }
                if (GUILayout.Button("+10,000", btnWarningStyle, GUILayout.Width(70), GUILayout.Height(22))) { AddGold(10000); }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("<color=#ffcc00>⚠️ Chưa tải được ProgressData của SaveData hiện tại.</color>");
            }

            GUILayout.Space(10);

            // --- CÁC NÚT LƯU ĐĨA TỨC THỜI ---
            GUILayout.Label("💾 <b>THAO TÁC GHI / TẢI TỨC THỜI XUỐNG Ổ ĐĨA:</b>", cardTitleStyle);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("💾 GHI SAVE NGAY (Save To Disk Sync)", btnPrimaryStyle, GUILayout.Height(32)))
            {
                if (sm != null)
                {
                    sm.SaveToDiskSync();
                    commandOutput = "💾 Đã ghi đè SaveData thành công xuống đĩa (Sync)!";
                }
            }

            if (GUILayout.Button("📂 Tải Lại Từ Disk (Reload Save)", btnWarningStyle, GUILayout.Height(32)))
            {
                if (sm != null)
                {
                    sm.LoadFromDisk();
                    commandOutput = "📂 Đã tải lại SaveData từ đĩa!";
                }
            }

            if (GUILayout.Button("🗑️ Xóa / Reset Slot Này", btnDangerStyle, GUILayout.Height(32)))
            {
                if (sm != null)
                {
                    sm.DeleteSlot(sm.CurrentSlotIndex);
                    sm.LoadFromDisk();
                    commandOutput = $"🗑️ Đã xóa và reset dữ liệu Slot {sm.CurrentSlotIndex}!";
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        #endregion

        #region ====== TAB 7: PERFORMANCE & SYSTEM STATS ======

        private void DrawPerformanceTab()
        {
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("📊 <b>THÔNG SỐ HỆ THỐNG & HIỆU NĂNG</b>", cardTitleStyle);
            GUILayout.Space(4);

            GUILayout.Label($"<b>FPS:</b> <color=#00ff88><b>{fps:F1}</b></color> <color=#888888>({deltaTime * 1000.0f:F1} ms)</color> | <b>TimeScale:</b> {Time.timeScale:F2}x");
            GUILayout.Label($"<b>Trạng Thái Game:</b> <color=#00e5ff>{GameManager.Instance?.CurrentState.ToString() ?? "N/A"}</color>");
            GUILayout.Label($"<b>Scene Hiện Tại:</b> {SceneManager.GetActiveScene().name}");
            GUILayout.Label($"<b>Độ Phân Giải Màn Hình:</b> {Screen.width} x {Screen.height} ({Screen.currentResolution.refreshRateRatio.value:F0}Hz)");
            GUILayout.Label($"<b>Hệ Điều Hành:</b> {SystemInfo.operatingSystem}");
            GUILayout.Label($"<b>GPU:</b> {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB)");
            GUILayout.Label($"<b>RAM Hệ Thống:</b> {SystemInfo.systemMemorySize} MB");

            GUILayout.Space(8);
            if (GUILayout.Button("🔄 Reload Scene Hiện Tại", btnWarningStyle, GUILayout.Height(30)))
            {
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            GUILayout.EndVertical();

            GUILayout.Space(4);

            // Frame Rate & V-Sync Controls
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("🖥️ <b>CÀI ĐẶT TỐC ĐỘ KHUNG HÌNH (FPS LIMIT & V-SYNC)</b>", cardTitleStyle);
            GUILayout.Space(4);

            int currentTarget = Application.targetFrameRate;
            int vsync = QualitySettings.vSyncCount;
            string targetStr = currentTarget == -1 ? "Không Giới Hạn (Unlimited)" : $"{currentTarget} FPS";
            string vsyncStr = vsync > 0 ? $"<color=#00ff88>BẬT (Theo Màn Hình: {Screen.currentResolution.refreshRateRatio.value:F0}Hz)</color>" : "<color=#ffcc00>TẮT (Theo Target FPS)</color>";

            GUILayout.Label($"<b>Target FPS:</b> <color=#00e5ff>{targetStr}</color> | <b>V-Sync:</b> {vsyncStr}");
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Chọn FPS Nhanh:", GUILayout.Width(110));

            if (GUILayout.Button("144 FPS", (currentTarget == 144 && vsync == 0) ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26)))
            {
                SetFPS(144, false);
            }
            if (GUILayout.Button("120 FPS", (currentTarget == 120 && vsync == 0) ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26)))
            {
                SetFPS(120, false);
            }
            if (GUILayout.Button("60 FPS", (currentTarget == 60 && vsync == 0) ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26)))
            {
                SetFPS(60, false);
            }
            if (GUILayout.Button("240 FPS", (currentTarget == 240 && vsync == 0) ? btnToggleOnStyle : btnNormalStyle, GUILayout.Height(26)))
            {
                SetFPS(240, false);
            }
            if (GUILayout.Button("🔓 Max (Unlimited)", (currentTarget == -1 && vsync == 0) ? btnToggleOnStyle : btnPrimaryStyle, GUILayout.Height(26)))
            {
                SetFPS(-1, false);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            bool isVsyncOn = vsync > 0;
            if (GUILayout.Button(isVsyncOn ? "🔄 V-Sync: ĐANG BẬT (Khóa theo màn hình)" : "🔄 V-Sync: ĐANG TẮT (Dùng Target FPS)", isVsyncOn ? btnWarningStyle : btnNormalStyle, GUILayout.Height(26)))
            {
                SetFPS(currentTarget, !isVsyncOn);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(4);

            // DebugLogger Modules
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("⚙️ <b>QUẢN LÝ DEBUGLOGGER MODULES</b>", cardTitleStyle);
            GUILayout.Space(4);

            bool globalState = DebugLogger.GlobalDebugEnabled;
            bool newGlobalState = GUILayout.Toggle(globalState, " <b>BẬT TOÀN BỘ DEBUGLOGGER (Master Switch)</b>");
            if (newGlobalState != globalState) DebugLogger.GlobalDebugEnabled = newGlobalState;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bật Tất Cả", btnPrimaryStyle, GUILayout.Height(22))) DebugLogger.EnableAllModules();
            if (GUILayout.Button("Tắt Tất Cả", btnDangerStyle, GUILayout.Height(22))) DebugLogger.DisableAllModules();
            if (GUILayout.Button("Reset Mặc Định", btnWarningStyle, GUILayout.Height(22))) DebugLogger.ResetAllDebugSettings();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void SetFPS(int targetFps, bool enableVsync)
        {
            QualitySettings.vSyncCount = enableVsync ? 1 : 0;
            Application.targetFrameRate = enableVsync ? -1 : targetFps;

            if (Roguelite.SaveSystem.SaveManager.Instance != null && Roguelite.SaveSystem.SaveManager.Instance.CurrentSettingData != null)
            {
                Roguelite.SaveSystem.SaveManager.Instance.CurrentSettingData.enableVSync = enableVsync;
                Roguelite.SaveSystem.SaveManager.Instance.CurrentSettingData.targetFrameRate = targetFps;
                Roguelite.SaveSystem.SaveManager.Instance.SaveSettingData();
            }
            commandOutput = enableVsync ? "🔄 Đã BẬT V-Sync (Khóa theo tần số quét màn hình)!" : $"🖥️ Đã đặt Target FPS thành: {(targetFps == -1 ? "Không giới hạn" : targetFps + " FPS")}!";
        }

        #endregion

        #region ====== CHEAT LOGIC IMPLEMENTATIONS ======

        private void AddGold(int amount)
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            {
                if (SaveManager.Instance.CurrentSaveData.progressData == null)
                {
                    SaveManager.Instance.CurrentSaveData.progressData = new PlayerProgressData();
                }
                if (Roguelite.UpgradeSystem.PermanentUpgradeManager.Instance != null)
                {
                    Roguelite.UpgradeSystem.PermanentUpgradeManager.Instance.AddCurrency(amount);
                }
                else
                {
                    SaveManager.Instance.CurrentSaveData.progressData.totalCurrency += amount;
                }
                SaveManager.Instance.SaveToDiskSync();
                commandOutput = $"💰 Đã cộng +{amount} vàng (Tổng: {SaveManager.Instance.CurrentSaveData.progressData.totalCurrency}) & Lưu đĩa!";
                DebugLogger.LogSuccess($"[DebugTool] {commandOutput}");
            }
            else
            {
                commandOutput = "⚠️ Chưa khởi tạo SaveManager!";
            }
        }

        private void KillEnemiesInCurrentRoomOnly()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                commandOutput = "❌ Không tìm thấy Player!";
                return;
            }

            Vector3 playerPos = player.transform.position;
            RoomManager[] rooms = FindObjectsOfType<RoomManager>();
            RoomManager activeRoom = null;

            foreach (var room in rooms)
            {
                if (room.IsPlayerInsideRoomInnerBounds(playerPos))
                {
                    activeRoom = room;
                    break;
                }
            }

            if (activeRoom == null)
            {
                float minDistance = float.MaxValue;
                foreach (var room in rooms)
                {
                    float dist = Vector3.Distance(playerPos, room.transform.position);
                    if (dist < minDistance && dist < 20f)
                    {
                        minDistance = dist;
                        activeRoom = room;
                    }
                }
            }

            if (activeRoom == null)
            {
                commandOutput = "⚠️ Player không đứng trong căn phòng nào!";
                return;
            }

            int count = 0;
            EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
            
            foreach (var enemy in allEnemies)
            {
                if (enemy == null || enemy.gameObject == null) continue;

                bool isChildOfRoom = enemy.transform.IsChildOf(activeRoom.transform);
                bool isInsideBounds = activeRoom.IsPlayerInsideRoomInnerBounds(enemy.transform.position);

                if (isChildOfRoom || isInsideBounds)
                {
                    enemy.TakeDamage(99999f);
                    count++;
                }
            }

            commandOutput = $"✅ Đã tiêu diệt {count} quái trong phòng [{activeRoom.gameObject.name}]!";
            DebugLogger.LogSuccess($"[DebugTool] {commandOutput}");
        }

        private void ClearCurrentRoomDoors()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null) return;

            RoomManager[] rooms = FindObjectsOfType<RoomManager>();
            foreach (var room in rooms)
            {
                if (room.IsPlayerInsideRoomInnerBounds(player.transform.position))
                {
                    room.OnRoomCleared();
                    commandOutput = $"✅ Đã mở tất cả cửa & dọn sạch phòng [{room.gameObject.name}]!";
                    return;
                }
            }
            commandOutput = "⚠️ Không tìm thấy phòng hiện tại!";
        }

        private void SkipToBossRoom()
        {
            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            if (mapGen != null)
            {
                mapGen.TeleportPlayerToBossRoom();
                commandOutput = "🚀 Đã dịch chuyển đến phòng Boss!";
            }
            else
            {
                commandOutput = "❌ Không tìm thấy MapGenerator trong Scene!";
            }
        }

        private void SkipToNextRoom()
        {
            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            var player = FindObjectOfType<PlayerController>();
            if (mapGen == null || player == null)
            {
                commandOutput = "❌ Không tìm thấy MapGenerator hoặc Player!";
                return;
            }

            RoomManager[] rooms = FindObjectsOfType<RoomManager>();
            if (rooms.Length <= 1)
            {
                commandOutput = "⚠️ Không có phòng tiếp theo!";
                return;
            }

            RoomManager targetRoom = null;
            float currentDist = float.MaxValue;

            foreach (var room in rooms)
            {
                float dist = Vector3.Distance(player.transform.position, room.transform.position);
                if (dist > 5f && dist < currentDist)
                {
                    currentDist = dist;
                    targetRoom = room;
                }
            }

            if (targetRoom != null)
            {
                player.transform.position = new Vector3(targetRoom.transform.position.x, targetRoom.transform.position.y, 0f);
                commandOutput = $"🚀 Đã dịch chuyển đến phòng tiếp theo: {targetRoom.gameObject.name}!";
            }
            else
            {
                commandOutput = "⚠️ Không tìm thấy phòng thích hợp!";
            }
        }

        private void SetDamageMultiplier(float multiplier)
        {
            currentDamageMultiplier = multiplier;
            commandOutput = $"🗡️ Đã chỉnh hệ số sát thương Player x{multiplier}!";
        }

        private void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            string cmd = input.Trim().ToLower();

            if (cmd == "help")
            {
                commandOutput = "Lệnh: god, heal, gold [num], timescale [num], damage [mult], kill, skipboss, nextroom, meteor, phase [0-2], save, reloadsave";
            }
            else if (cmd == "god" || cmd == "godmode")
            {
                PlayerStats stats = FindObjectOfType<PlayerStats>();
                if (stats != null)
                {
                    stats.IsGodMode = !stats.IsGodMode;
                    commandOutput = $"GodMode: {(stats.IsGodMode ? "BẬT" : "TẮT")}";
                }
            }
            else if (cmd == "heal")
            {
                PlayerStats stats = FindObjectOfType<PlayerStats>();
                if (stats != null) stats.Heal(9999f);
                commandOutput = "Đã hồi đầy HP!";
            }
            else if (cmd.StartsWith("timescale") || cmd.StartsWith("speed"))
            {
                string[] parts = cmd.Split(' ');
                if (parts.Length > 1 && float.TryParse(parts[1], out float scale)) SetTimeScale(scale);
            }
            else if (cmd.StartsWith("gold"))
            {
                string[] parts = cmd.Split(' ');
                int amount = (parts.Length > 1 && int.TryParse(parts[1], out int val)) ? val : 1000;
                AddGold(amount);
            }
            else if (cmd == "meteor")
            {
                WorldBoss wb = FindObjectOfType<WorldBoss>();
                if (wb != null) { wb.ForceTriggerMeteorRain(); commandOutput = "🔥 Đã kích hoạt Mưa Thiên Thạch!"; }
            }
            else if (cmd.StartsWith("phase"))
            {
                string[] parts = cmd.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[1], out int p))
                {
                    WorldBoss wb = FindObjectOfType<WorldBoss>();
                    if (wb != null) { wb.ForceSetPhase(p); commandOutput = $"🌟 Đã ép Boss sang Phase {p}!"; }
                }
            }
            else if (cmd == "save")
            {
                SaveManager.Instance?.SaveToDiskSync();
                commandOutput = "💾 Đã ghi SaveData xuống đĩa thành công!";
            }
            else if (cmd == "reloadsave")
            {
                SaveManager.Instance?.LoadFromDisk();
                commandOutput = "📂 Đã tải lại SaveData từ đĩa!";
            }
            else if (cmd == "kill")
            {
                KillEnemiesInCurrentRoomOnly();
            }
            else if (cmd == "skipboss" || cmd == "boss")
            {
                SkipToBossRoom();
            }
            else if (cmd == "nextroom" || cmd == "next")
            {
                SkipToNextRoom();
            }
            else
            {
                commandOutput = $"❓ Lệnh '{cmd}' không hợp lệ. Gõ 'help' để xem hỗ trợ.";
            }
        }

        #endregion
    }
}
