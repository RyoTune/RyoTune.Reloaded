using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Common;

namespace RyoTune.Reloaded;

/// <summary>
/// Utility class for pattern scanning and hooking with <see cref="IReloadedHooks"/>.
/// </summary>
public static class ScanHooks
{
    internal const string PATTERNS_FILE = "scan-patterns.ini";

    private static readonly List<ScanListener> _listeners = [];
    private static readonly Dictionary<string, string> _latestPatterns = [];
    private static Dictionary<string, string> _localPatterns = [];

    private static IReloadedHooks? _hooks;
    private static IStartupScanner? _scanner;

    internal static void Initialize(IModLoader modLoader)
    {
        modLoader.GetController<IReloadedHooks>().TryGetTarget(out _hooks);
        modLoader.GetController<IStartupScanner>().TryGetTarget(out _scanner);

        var localPatternsFile = Path.Join(Project.Folder, PATTERNS_FILE);
        if (File.Exists(localPatternsFile))
        {
            _localPatterns = ParsePatternsFile(localPatternsFile);
        }
    }

    internal static void RegisterPatterns(string patternsMod, string patternsFile)
    {
        var patterns = ParsePatternsFile(patternsFile);
        foreach (var item in patterns)
        {
            Add(item.Key, item.Value, false);
            Log.Information($"Registered Pattern || Scan: {item.Key} || From: {patternsMod}");
        }
    }

    /// <summary>
    /// Add a scan with the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="pattern">Scan pattern.</param>
    public static void Add(string scanId, string pattern) => Add(scanId, pattern, true);

    /// <summary>
    /// Add a scan with the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="pattern">Pattern to scan for.</param>
    /// <param name="onSuccess">Callback given result and hooks, on success.</param>
    public static void Add(string scanId, string pattern, Action<IReloadedHooks, nint> onSuccess)
    {
        Add(scanId, pattern, true);
        _listeners.Add(new ScanListener(scanId, result => onSuccess(_hooks!, result)));
    }

    /// <summary>
    /// Add a listener for the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="success">Success action with result.</param>
    public static void Listen(string scanId, Action<IReloadedHooks, nint> success) => _listeners.Add(new ScanListener(scanId, result => success(_hooks!, result)));

    /// <summary>
    /// Add a scan with the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="pattern">Scan pattern.</param>
    /// <param name="allowLocalOverride">Allow a locally provided pattern to override the given <paramref name="pattern"/>.</param>
    private static void Add(string scanId, string pattern, bool allowLocalOverride)
    {
        // Prefer using a pattern provided by the mod project in: MOD_FOLDER/Project/scan-patterns.ini
        if (allowLocalOverride && _localPatterns.TryGetValue(scanId, out var newPattern)) pattern = newPattern;

        _scanner!.Scan(scanId, pattern, result => OnScanSuccess(scanId, pattern, result), () => OnScanFailure(scanId, pattern));
        _latestPatterns[scanId] = pattern;
    }

    private static void OnScanSuccess(string scanId, string pattern, nint result)
    {
        // Ignore if scan was not for the latest pattern of Scan ID.
        if (_latestPatterns[scanId] != pattern) return;

        Log.Information($"\"{scanId}\" found at: 0x{result:X}");

        var scanListeners = _listeners.Where(x => x.ScanId == scanId).ToArray();
        foreach (var listener in scanListeners)
        {
            listener.OnSuccess(result);
        }
    }

    private static void OnScanFailure(string scanId, string pattern)
    {
        // Only log as failure if it's the current pattern for the Scan ID.
        if (_latestPatterns[scanId] == pattern) Log.Error($"Failed to find pattern for \"{scanId}\". Pattern: {pattern}");
    }

    private static Dictionary<string, string> ParsePatternsFile(string file)
    {
        try
        {
            var data = IniParsing.Instance.ReadFile(file);
            var patterns = data.Sections.FirstOrDefault(x => x.SectionName == Project.Id);
            if (patterns == null) return [];

            return patterns.Keys.ToDictionary(x => x.KeyName, x => x.Value);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to load patterns from file.\nFile: {file}");
        }

        return [];
    }

    private record ScanListener(string ScanId, Action<nint> OnSuccess);
}