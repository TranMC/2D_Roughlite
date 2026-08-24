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
    /// Công cụ Debug & Cheat Runtime (IMGUI) phục vụ Playtest trên bản Build thực tế (Standalone EXE/APK).
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
        private Rect windowRect = new Rect(20, 20, 780, 520);
        private int activeTab = 0;
        private readonly string[] tabNames = { "📜 Console Logs", "⚡ Cheats & Actions", "⚙️ Debug Modules", "📊 Performance Stats" };

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
            windowRect = GUI.Window(99999, windowRect, DrawConsoleWindow, "<color=#00e5ff><b>🛠️ IN-GAME DEBUG TOOL</b></color> <color=#888888>(2D Roughlite)</color>", windowStyle);
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
            texWindowBg = MakeTex(2, 2, new Color(0.08f, 0.10f, 0.14f, 0.96f)); // Dark Navy Slate
            texCardBg = MakeTex(2, 2, new Color(0.12f, 0.15f, 0.21f, 0.90f));   // Card Panel
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
            windowStyle.padding = new RectOffset(12, 12, 28, 12);
            windowStyle.normal.textColor = new Color(0.95f, 0.96f, 0.98f);
            windowStyle.fontSize = 13;
            windowStyle.fontStyle = FontStyle.Bold;
            windowStyle.richText = true;

            // Tab Styles
            tabActiveStyle = new GUIStyle(GUI.skin.button);
            tabActiveStyle.normal.background = texTabActive;
            tabActiveStyle.hover.background = texTabActive;
            tabActiveStyle.normal.textColor = Color.white;
            tabActiveStyle.fontSize = 12;
            tabActiveStyle.fontStyle = FontStyle.Bold;
            tabActiveStyle.margin = new RectOffset(2, 2, 2, 2);

            tabInactiveStyle = new GUIStyle(GUI.skin.button);
            tabInactiveStyle.normal.background = texTabInactive;
            tabInactiveStyle.hover.background = texTabHover;
            tabInactiveStyle.normal.textColor = new Color(0.70f, 0.74f, 0.82f);
            tabInactiveStyle.fontSize = 12;
            tabInactiveStyle.fontStyle = FontStyle.Normal;
            tabInactiveStyle.margin = new RectOffset(2, 2, 2, 2);

            closeBtnStyle = new GUIStyle(GUI.skin.button);
            closeBtnStyle.normal.background = texBtnClose;
            closeBtnStyle.hover.background = texBtnCloseHover;
            closeBtnStyle.normal.textColor = Color.white;
            closeBtnStyle.fontSize = 12;
            closeBtnStyle.fontStyle = FontStyle.Bold;
            closeBtnStyle.margin = new RectOffset(4, 0, 2, 2);

            // Card Style
            cardBoxStyle = new GUIStyle();
            cardBoxStyle.normal.background = texCardBg;
            cardBoxStyle.padding = new RectOffset(12, 12, 10, 10);
            cardBoxStyle.margin = new RectOffset(0, 0, 4, 8);

            cardTitleStyle = new GUIStyle(GUI.skin.label);
            cardTitleStyle.fontSize = 12;
            cardTitleStyle.fontStyle = FontStyle.Bold;
            cardTitleStyle.normal.textColor = new Color(0.00f, 0.88f, 1.0f); // Neon Cyan
            cardTitleStyle.richText = true;

            // Button Styles
            btnNormalStyle = new GUIStyle(GUI.skin.button);
            btnNormalStyle.normal.background = texBtnNormal;
            btnNormalStyle.hover.background = texBtnHover;
            btnNormalStyle.normal.textColor = Color.white;
            btnNormalStyle.fontSize = 11;
            btnNormalStyle.fontStyle = FontStyle.Normal;
            btnNormalStyle.margin = new RectOffset(2, 2, 2, 2);

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
            inputFieldStyle.fontSize = 12;
            inputFieldStyle.padding = new RectOffset(8, 8, 5, 5);

            // Log Container Box
            logBoxStyle = new GUIStyle();
            logBoxStyle.normal.background = texLogBg;
            logBoxStyle.padding = new RectOffset(8, 8, 8, 8);

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
                if (GUILayout.Button(tabNames[i], currentTabStyle, GUILayout.Height(32)))
                {
                    activeTab = i;
                }
            }

            if (GUILayout.Button("✕ Đóng", closeBtnStyle, GUILayout.Width(75), GUILayout.Height(32)))
            {
                showConsole = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // Nội dung theo Tab
            switch (activeTab)
            {
                case 0:
                    DrawLogsTab();
                    break;
                case 1:
                    DrawCheatsTab();
                    break;
                case 2:
                    DrawDebugModulesTab();
                    break;
                case 3:
                    DrawPerformanceTab();
                    break;
            }

            // Lệnh kéo rê cửa sổ khi nhấp vào thanh tiêu đề
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 28));
        }

        #endregion

        #region ====== TAB 1: LOG CONSOLE ======

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

            if (GUILayout.Button("🗑️ Xóa Log", btnNormalStyle, GUILayout.Width(90), GUILayout.Height(24)))
            {
                logEntries.Clear();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // Bảng danh sách Log
            GUILayout.BeginVertical(logBoxStyle);
            logScrollPos = GUILayout.BeginScrollView(logScrollPos, GUILayout.Height(360));
            
            foreach (var log in logEntries)
            {
                if (log.type == LogType.Log && !showInfoLogs) continue;
                if (log.type == LogType.Warning && !showWarningLogs) continue;
                if ((log.type == LogType.Error || log.type == LogType.Exception) && !showErrorLogs) continue;

                string colorHex = "#ffffff";
                string typePrefix = "[INFO]";

                switch (log.type)
                {
                    case LogType.Warning:
                        colorHex = "#ffcc00";
                        typePrefix = "[WARN]";
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                        colorHex = "#ff4d4d";
                        typePrefix = "[ERR!]";
                        break;
                    default:
                        colorHex = "#e0e0e0";
                        typePrefix = "[LOG]";
                        break;
                }

                GUILayout.Label($"<color=#777777>[{log.timestamp}]</color> <color={colorHex}><b>{typePrefix}</b> {log.message}</color>");
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        #endregion

        #region ====== TAB 2: CHEATS & QUICK ACTIONS ======

        private void DrawCheatsTab()
        {
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("⚡ <b>THAO TÁC NHANH (QUICK ACTIONS)</b>", cardTitleStyle);
            GUILayout.Space(4);

            PlayerStats playerStats = FindObjectOfType<PlayerStats>();

            // --- HÀNG 1: BẤT TỬ & HỒI MÁU ---
            GUILayout.BeginHorizontal();

            bool currentGodMode = playerStats != null && playerStats.IsGodMode;
            GUIStyle godStyle = currentGodMode ? btnToggleOnStyle : btnToggleOffStyle;
            if (GUILayout.Button(currentGodMode ? "🛡️ God Mode: ON" : "🛡️ God Mode: OFF", godStyle, GUILayout.Height(32)))
            {
                if (playerStats != null)
                {
                    playerStats.IsGodMode = !playerStats.IsGodMode;
                    DebugLogger.LogSuccess($"[DebugTool] GodMode toggled: {playerStats.IsGodMode}");
                }
            }

            if (GUILayout.Button("❤️ Hồi Đầy Máu", btnPrimaryStyle, GUILayout.Height(32)))
            {
                if (playerStats != null)
                {
                    playerStats.Heal(99999f);
                    DebugLogger.LogSuccess("[DebugTool] Đã hồi đầy máu!");
                }
            }

            if (GUILayout.Button("💰 +1,000 Vàng", btnWarningStyle, GUILayout.Height(32)))
            {
                AddGold(1000);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // --- HÀNG 2: DIỆT QUÁI PHÒNG HIỆN TẠI & SKIP ROOM ---
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("☠️ Diệt Quái Phòng Này", btnDangerStyle, GUILayout.Height(32)))
            {
                KillEnemiesInCurrentRoomOnly();
            }

            if (GUILayout.Button("🔓 Mở Cửa Phòng Này", btnNormalStyle, GUILayout.Height(32)))
            {
                ClearCurrentRoomDoors();
            }

            if (GUILayout.Button("👑 Skip Tới Phòng Boss", btnWarningStyle, GUILayout.Height(32)))
            {
                SkipToBossRoom();
            }

            if (GUILayout.Button("🚪 Skip Sang Phòng Tiếp", btnNormalStyle, GUILayout.Height(32)))
            {
                SkipToNextRoom();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // --- HÀNG 3: CHỈNH TỐC ĐỘ & SÁT THƯƠNG ---
            GUILayout.Label("🏃 <b>TỐC ĐỘ DI CHUYỂN & SÁT THƯƠNG</b>", cardTitleStyle);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Speed x1.5", btnNormalStyle, GUILayout.Height(28))) SetSpeedMultiplier(1.5f);
            if (GUILayout.Button("Speed x2.0", btnNormalStyle, GUILayout.Height(28))) SetSpeedMultiplier(2.0f);
            if (GUILayout.Button("Speed x3.0", btnNormalStyle, GUILayout.Height(28))) SetSpeedMultiplier(3.0f);
            if (GUILayout.Button("Reset Speed", btnDangerStyle, GUILayout.Height(28))) SetSpeedMultiplier(1.0f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Damage x1.5", btnNormalStyle, GUILayout.Height(28))) SetDamageMultiplier(1.5f);
            if (GUILayout.Button("Damage x2.0", btnNormalStyle, GUILayout.Height(28))) SetDamageMultiplier(2.0f);
            if (GUILayout.Button("Damage x5.0", btnNormalStyle, GUILayout.Height(28))) SetDamageMultiplier(5.0f);
            if (GUILayout.Button("Reset Damage", btnDangerStyle, GUILayout.Height(28))) SetDamageMultiplier(1.0f);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(6);

            // --- KHUNG NHẬP LỆNH COMMAND LINE ---
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("💻 <b>KHUNG NHẬP LỆNH (COMMAND CONSOLE)</b>", cardTitleStyle);
            GUILayout.Space(4);
            
            GUILayout.BeginHorizontal();
            commandInput = GUILayout.TextField(commandInput, inputFieldStyle, GUILayout.Height(28));
            if (GUILayout.Button("Gửi Lệnh", btnPrimaryStyle, GUILayout.Width(100), GUILayout.Height(28)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                ExecuteCommand(commandInput);
                commandInput = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label($"<color=#00e5ff>{commandOutput}</color>");
            GUILayout.EndVertical();
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
                SaveManager.Instance.CurrentSaveData.progressData.totalCurrency += amount;
                SaveManager.Instance.SaveToDiskSync();
                commandOutput = $"💰 Đã cộng +{amount} vàng (Tổng: {SaveManager.Instance.CurrentSaveData.progressData.totalCurrency})!";
                DebugLogger.LogSuccess($"[DebugTool] {commandOutput}");
            }
            else
            {
                commandOutput = "⚠️ Chưa khởi tạo SaveManager!";
            }
        }

        /// <summary>
        /// TIÊU DIỆT QUÁI TRONG CĂN PHÒNG HIỆN TẠI (KIỂM SOÁT TỈ MỈ TRONG 1 ROOM, KHÔNG DIỆT TOÀN BẢN ĐỒ)
        /// </summary>
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

            // Tìm phòng mà Player đang thực sự đứng bên trong ranh giới
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
                // Fallback: Tìm phòng gần Player nhất trong khoảng bán kính 20 đơn vị
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

            // Tìm và tiêu diệt các quái vật thuộc phòng này
            int count = 0;
            EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
            
            foreach (var enemy in allEnemies)
            {
                if (enemy == null || enemy.gameObject == null) continue;

                // Kiểm tra xem quái có nằm trong ranh giới của activeRoom hay là con của activeRoom không
                bool isChildOfRoom = enemy.transform.IsChildOf(activeRoom.transform);
                bool isInsideBounds = activeRoom.IsPlayerInsideRoomInnerBounds(enemy.transform.position);

                if (isChildOfRoom || isInsideBounds)
                {
                    enemy.TakeDamage(99999f); // Tiêu diệt quái
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

            // Tìm phòng có khoảng cách xa hơn vị trí hiện tại của Player
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
                player.transform.position = targetRoom.transform.position;
                commandOutput = $"🚀 Đã dịch chuyển đến phòng tiếp theo: {targetRoom.gameObject.name}!";
            }
            else
            {
                commandOutput = "⚠️ Không tìm thấy phòng thích hợp!";
            }
        }

        private void SetSpeedMultiplier(float multiplier)
        {
            currentSpeedMultiplier = multiplier;
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.walkSpeed = 5f * multiplier;
                player.runSpeed = 8f * multiplier;
                commandOutput = $"⚡ Đã chỉnh tốc độ di chuyển x{multiplier}!";
            }
        }

        private void SetDamageMultiplier(float multiplier)
        {
            currentDamageMultiplier = multiplier;
            var attacks = FindObjectsOfType<Attack>();
            foreach (var attack in attacks)
            {
                if (attack.transform.IsChildOf(FindObjectOfType<PlayerController>().transform))
                {
                    attack.AttackDamage = 10f * multiplier;
                }
            }
            commandOutput = $"🗡️ Đã chỉnh sát thương Player x{multiplier}!";
        }

        private void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            string cmd = input.Trim().ToLower();

            if (cmd == "help")
            {
                commandOutput = "Lệnh: godmode, heal, gold [num], speed [mult], damage [mult], killroom, skipboss, nextroom";
            }
            else if (cmd == "godmode" || cmd == "god")
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
            else if (cmd.StartsWith("gold"))
            {
                string[] parts = cmd.Split(' ');
                int amount = (parts.Length > 1 && int.TryParse(parts[1], out int val)) ? val : 1000;
                AddGold(amount);
            }
            else if (cmd.StartsWith("speed"))
            {
                string[] parts = cmd.Split(' ');
                float mult = (parts.Length > 1 && float.TryParse(parts[1], out float val)) ? val : 2.0f;
                SetSpeedMultiplier(mult);
            }
            else if (cmd.StartsWith("damage"))
            {
                string[] parts = cmd.Split(' ');
                float mult = (parts.Length > 1 && float.TryParse(parts[1], out float val)) ? val : 2.0f;
                SetDamageMultiplier(mult);
            }
            else if (cmd == "killroom" || cmd == "kill")
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

        #region ====== TAB 3: DEBUG LOGGER MODULES ======

        private void DrawDebugModulesTab()
        {
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("⚙️ <b>QUẢN LÝ DEBUGLOGGER MODULES</b>", cardTitleStyle);
            GUILayout.Space(6);

            bool globalState = DebugLogger.GlobalDebugEnabled;
            bool newGlobalState = GUILayout.Toggle(globalState, " <b>BẬT TOÀN BỘ DEBUGLOGGER (Master Switch)</b>");
            if (newGlobalState != globalState)
            {
                DebugLogger.GlobalDebugEnabled = newGlobalState;
            }

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bật Tất Cả Modules", btnPrimaryStyle, GUILayout.Height(28))) DebugLogger.EnableAllModules();
            if (GUILayout.Button("Tắt Tất Cả Modules", btnDangerStyle, GUILayout.Height(28))) DebugLogger.DisableAllModules();
            if (GUILayout.Button("Reset Mặc Định", btnWarningStyle, GUILayout.Height(28))) DebugLogger.ResetAllDebugSettings();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // Các module mặc định của project
            string[] knownModules = { "PlayerStats", "PlayerController", "Enemy", "RoomSystem", "Combat", "SaveSystem", "UpgradeSystem" };

            foreach (var mod in knownModules)
            {
                bool currentState = DebugLogger.GetModuleDebug(mod);
                bool newState = GUILayout.Toggle(currentState, $" Module: <color=#00e5ff><b>{mod}</b></color>");
                if (newState != currentState)
                {
                    DebugLogger.SetModuleDebug(mod, newState);
                }
            }

            GUILayout.EndVertical();
        }

        #endregion

        #region ====== TAB 4: PERFORMANCE & SYSTEM STATS ======

        private void DrawPerformanceTab()
        {
            GUILayout.BeginVertical(cardBoxStyle);
            GUILayout.Label("📊 <b>THÔNG SỐ HỆ THỐNG & HIỆU NĂNG</b>", cardTitleStyle);
            GUILayout.Space(6);

            GUILayout.Label($"<b>FPS:</b> <color=#00ff88><b>{fps:F1}</b></color> <color=#888888>({deltaTime * 1000.0f:F1} ms)</color>");
            GUILayout.Label($"<b>Trạng Thái Game:</b> <color=#00e5ff>{GameManager.Instance?.CurrentState.ToString() ?? "N/A"}</color>");
            GUILayout.Label($"<b>Scene Hiện Tại:</b> {SceneManager.GetActiveScene().name}");
            GUILayout.Label($"<b>Độ Phân Giải Màn Hình:</b> {Screen.width} x {Screen.height} ({Screen.currentResolution.refreshRateRatio.value:F0}Hz)");
            GUILayout.Label($"<b>Hệ Điều Hành:</b> {SystemInfo.operatingSystem}");
            GUILayout.Label($"<b>GPU:</b> {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB)");
            GUILayout.Label($"<b>RAM Hệ Thống:</b> {SystemInfo.systemMemorySize} MB");

            GUILayout.Space(12);
            if (GUILayout.Button("🔄 Reload Scene Hiện Tại", btnWarningStyle, GUILayout.Height(32)))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}
