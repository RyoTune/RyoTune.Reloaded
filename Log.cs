using Reloaded.Mod.Interfaces;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace RyoTune.Reloaded;

public static class Log
{
    private static string name = "Mod";
    private static ILogger? log;
    private static Color color = Color.White;
    private static bool useAsync;

    public static void Init(string name, ILogger log, bool alwaysAsync)
    {
        Log.name = name;
        Log.log = log;
        useAsync = alwaysAsync;
        color = GetColor(name);
    }

    public static void Init(string name, ILogger log, Color color, bool useAsync)
    {
        Log.name = name;
        Log.log = log;
        Log.color = color;
        Log.useAsync = useAsync;
    }

    public static LogLevel LogLevel { get; set; } = LogLevel.Information;

    public static void Verbose(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Debug)
        {
            LogMessage(LogLevel.Verbose, message, useAsync);
        }
    }

    public static void Debug(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Information)
        {
            LogMessage(LogLevel.Debug, message, useAsync);
        }
    }

    public static void Information(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Warning)
        {
            LogMessage(LogLevel.Information, message, useAsync);
        }
    }

    public static void Warning(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Error)
        {
            LogMessage(LogLevel.Warning, message, useAsync);
        }
    }
    public static void Error(Exception ex, string message, bool useAsync = false)
    {
        LogMessage(LogLevel.Error, $"{message}\n{ex.Message}\n{ex.StackTrace}", useAsync);
    }

    public static void Error(string message, bool useAsync = false)
    {
        LogMessage(LogLevel.Error, message, useAsync);
    }

    private static void LogMessage(LogLevel level, string message, bool useAsync = false)
    {
        var color =
            level == LogLevel.Debug ? Color.LightGreen :
            level == LogLevel.Information ? Log.color :
            level == LogLevel.Error ? Color.Red :
            level == LogLevel.Warning ? Color.LightGoldenrodYellow :
            Color.White;

        if (useAsync || Log.useAsync)
        {
            log?.WriteLineAsync(FormatMessage(level, message), color);
        }
        else
        {
            log?.WriteLine(FormatMessage(level, message), color);
        }
    }

    private static string FormatMessage(LogLevel level, string message)
    {
        var levelStr =
            level == LogLevel.Verbose ? "[VRB]" :
            level == LogLevel.Debug ? "[DBG]" :
            level == LogLevel.Information ? "[INF]" :
            level == LogLevel.Warning ? "[WRN]" :
            level == LogLevel.Error ? "[ERR]" : string.Empty;

        return $"[{name}] {levelStr} {message}";
    }

    private static Color GetColor(string str)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(str));
        var bytes = BitConverter.GetBytes(BitConverter.ToUInt32(hash));
        var color = Color.FromArgb(0xFF, bytes[0], bytes[1], bytes[2]).WithMinBrightness(0.85);
        return color;
    }
}

public enum LogLevel
{
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
}