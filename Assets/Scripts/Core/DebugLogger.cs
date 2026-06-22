using System;
using System.Collections.Generic;
using UnityEngine;

public static class DebugLogger
{
    // Bật/tắt debug log toàn bộ game (Master switch)
    private static bool globalDebugEnabled = true;

    // Dictionary lưu trạng thái debug của từng module/chức năng
    private static Dictionary<string, bool> moduleDebugStates = new Dictionary<string, bool>();

    // Các loại log
    public enum LogType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public static bool GlobalDebugEnabled
    {
        get { return globalDebugEnabled; }
        set { globalDebugEnabled = value; }
    }

    public static void SetModuleDebug(string moduleName, bool enabled)
    {
        if (moduleDebugStates.ContainsKey(moduleName))
        {
            moduleDebugStates[moduleName] = enabled;
        }
        else
        {
            moduleDebugStates.Add(moduleName, enabled);
        }
    }

    public static bool GetModuleDebug(string moduleName)
    {
        if (moduleDebugStates.ContainsKey(moduleName))
        {
            return moduleDebugStates[moduleName];
        }
        return false; // Mặc định tắt nếu chưa đăng ký
    }

    private static bool ShouldLog(string moduleName)
    {
        if (!globalDebugEnabled) return false;
        
        if (string.IsNullOrEmpty(moduleName)) return true; // Log chung không cần check module
        
        return GetModuleDebug(moduleName);
    }

    public static void Log(string message, string moduleName = "")
    {
        if (!ShouldLog(moduleName)) return;
        
        string prefix = string.IsNullOrEmpty(moduleName) ? "" : $"[{moduleName}] ";
        Debug.Log($"{prefix}{message}");
    }

    public static void Log(string message, string moduleName, LogType logType)
    {
        if (!ShouldLog(moduleName)) return;
        
        string coloredMessage = FormatMessage(message, moduleName, logType);
        
        switch (logType)
        {
            case LogType.Warning:
                Debug.LogWarning(coloredMessage);
                break;
            case LogType.Error:
                Debug.Log(coloredMessage);
                break;
            default:
                Debug.Log(coloredMessage);
                break;
        }
    }

   public static void LogWarning(string message, string moduleName = "")
    {
        if (!ShouldLog(moduleName)) return;
        
        string prefix = string.IsNullOrEmpty(moduleName) ? "" : $"[{moduleName}] ";
        Debug.LogWarning($"{prefix}{message}");
    }

    public static void LogError(string message, string moduleName = "")
    {
        if (!ShouldLog(moduleName)) return;
        
        string prefix = string.IsNullOrEmpty(moduleName) ? "" : $"[{moduleName}] ";
        Debug.Log($"<color=red>{prefix}{message}</color>");
    }

    public static void LogSuccess(string message, string moduleName = "")
    {
        if (!ShouldLog(moduleName)) return;
        
        string prefix = string.IsNullOrEmpty(moduleName) ? "" : $"[{moduleName}] ";
        Debug.Log($"<color=lime>{prefix}{message}</color>");
    }

    private static string FormatMessage(string message, string moduleName, LogType logType)
    {
        string prefix = string.IsNullOrEmpty(moduleName) ? "" : $"[{moduleName}] ";
        string color = GetColorForLogType(logType);
        
        if (!string.IsNullOrEmpty(color))
        {
            return $"<color={color}>{prefix}{message}</color>";
        }
        
        return $"{prefix}{message}";
    }

    private static string GetColorForLogType(LogType logType)
    {
        switch (logType)
        {
            case LogType.Info:
                return "cyan";
            case LogType.Warning:
                return "yellow";
            case LogType.Error:
                return "red";
            case LogType.Success:
                return "lime";
            default:
                return "";
        }
    }

    public static void PrintModuleStates()
    {
        if (!globalDebugEnabled)
        {
            Debug.Log("Global Debug đã bị TẮT");
            return;
        }

        Debug.Log("=== Trạng thái các Module Debug ===");
        Debug.Log($"Global Debug: {globalDebugEnabled}");
        
        foreach (var kvp in moduleDebugStates)
        {
            string status = kvp.Value ? "<color=lime>ENABLED</color>" : "<color=red>DISABLED</color>";
            Debug.Log($"  {kvp.Key}: {status}");
        }
    }

    public static void ResetAllDebugSettings()
    {
        moduleDebugStates.Clear();
        globalDebugEnabled = true;
        Debug.Log("Tất cả cài đặt debug đã được đặt lại.");
    }

    public static void EnableAllModules()
    {
        List<string> keys = new List<string>(moduleDebugStates.Keys);
        foreach (string key in keys)
        {
            moduleDebugStates[key] = true;
        }
        Debug.Log("Tất cả module debug đã được bật.");
    }

    public static void DisableAllModules()
    {
        List<string> keys = new List<string>(moduleDebugStates.Keys);
        foreach (string key in keys)
        {
            moduleDebugStates[key] = false;
        }
        Debug.Log("Tất cả module debug đã được tắt.");
    }
}
