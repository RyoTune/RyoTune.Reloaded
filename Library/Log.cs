using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ILogger = Reloaded.Mod.Interfaces.ILogger;

namespace RyoTune.Reloaded;

/// <summary>
/// Basic log functionality using Reloaded's logger.
/// </summary>
public static class Log
{
    private static readonly Dictionary<LogLevel, string> _levelPrefixes = new()
    {
        [LogLevel.Verbose] = "[VRB]",
        [LogLevel.Debug] = "[DBG]",
        [LogLevel.Information] = "[INF]",
        [LogLevel.Warning] = "[WRN]",
        [LogLevel.Error] = "[ERR]",
    };

    private static readonly Dictionary<LogLevel, Color> _levelColors = new()
    {
        [LogLevel.Verbose] = Color.White,
        [LogLevel.Debug] = Color.LightGreen,
        [LogLevel.Information] = Color.White,
        [LogLevel.Warning] = Color.LightGoldenrodYellow,
        [LogLevel.Error] = Color.Red,
    };

    private static readonly Microsoft.Extensions.Logging.ILogger _mlog = new MLogger();
    private static string _name = "Mod";
    private static ILogger? _log;
    private static bool _useAsync;

    internal static void Init(string name, ILogger log, bool alwaysAsync)
    {
        _name = name;
        _log = log;
        _useAsync = alwaysAsync;
        _levelColors[LogLevel.Information] = GetColor(name);
    }

    internal static void Init(string name, ILogger log, Color color, bool useAsync)
    {
        _name = name;
        _log = log;
        _levelColors[LogLevel.Information] = color;
        _useAsync = useAsync;
    }

    /// <summary>
    /// Set the minimum level for logs to appear.
    /// </summary>
    public static LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Logs that are not notable and rarely useful.
    /// </summary>
    public static void Verbose(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Debug)
        {
            LogMessage(LogLevel.Verbose, message, useAsync);
        }
    }

    /// <summary>
    /// Logs that may be useful for debugging errors.
    /// </summary>
    public static void Debug(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Information)
        {
            LogMessage(LogLevel.Debug, message, useAsync);
        }
    }

    /// <summary>
    /// Logs that are almost always useful, such as indicating something has executed correctly.
    /// </summary>
    public static void Information(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Warning)
        {
            LogMessage(LogLevel.Information, message, useAsync);
        }
    }

    /// <summary>
    /// Logs indicating something incorrect or not ideal has occured, but nothing that would break mod functionality.
    /// </summary>
    public static void Warning(string message, bool useAsync = false)
    {
        if (LogLevel < LogLevel.Error)
        {
            LogMessage(LogLevel.Warning, message, useAsync);
        }
    }

    /// <summary>
    /// Logs indicating an error has occured, typically one that will break mod functionality.
    /// </summary>
    public static void Error(Exception ex, string message, bool useAsync = false)
    {
        LogMessage(LogLevel.Error, $"{message}\n{ex.Message}\n{ex.StackTrace}", useAsync);
    }

    /// <summary>
    /// Logs indicating an error has occured, typically one that will break mod functionality.
    /// </summary>
    public static void Error(string message, bool useAsync = false)
    {
        LogMessage(LogLevel.Error, message, useAsync);
    }

    private static void LogMessage(LogLevel level, string message, bool useAsync = false)
    {
        if (useAsync || _useAsync)
        {
            _log?.WriteLineAsync(FormatMessage(level, message), _levelColors[level]);
        }
        else
        {
            _log?.WriteLine(FormatMessage(level, message), _levelColors[level]);
        }
    }

    private static string FormatMessage(LogLevel level, string message) => $"[{_name}] {_levelPrefixes[level]} {message}";

    private static Color GetColor(string str)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(str));
        var bytes = BitConverter.GetBytes(BitConverter.ToUInt32(hash));
        var color = Color.FromArgb(0xFF, bytes[0], bytes[1], bytes[2]).WithMinBrightness(0.85);
        return color;
    }

    /// <summary>
    /// Gets the logger as an <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.
    /// </summary>
    public static Microsoft.Extensions.Logging.ILogger AsLogger() => _mlog;

    private class MLogger : Microsoft.Extensions.Logging.ILogger
    {
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    Verbose(formatter(state, exception));
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    Debug(formatter(state, exception));
                    break;
                case Microsoft.Extensions.Logging.LogLevel.None:
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    Information(formatter(state, exception));
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    Warning(formatter(state, exception));
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    Error(formatter(state, exception));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}

/// <summary>
/// Log severity level.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Logs that are not notable and rarely useful.
    /// </summary>
    Verbose,

    /// <summary>
    /// Logs that may be useful for debugging errors.
    /// </summary>
    Debug,

    /// <summary>
    /// Logs that are almost always useful, such as indicating something has executed correctly.
    /// </summary>
    Information,

    /// <summary>
    /// Logs indicating something incorrect or not ideal has occured, but nothing that would break mod functionality.
    /// </summary>
    Warning,

    /// <summary>
    /// Logs indicating an error has occured, typically one that will break mod functionality.
    /// </summary>
    Error,
}