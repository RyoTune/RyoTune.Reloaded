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
    private static readonly List<ScanListener> _listeners = [];
    private static readonly Dictionary<string, string> _scanPatterns = [];

    private static IReloadedHooks? _hooks;
    private static IStartupScanner? _scanner;

    internal static void Initialize(IModLoader modLoader)
    {
        modLoader.GetController<IReloadedHooks>().TryGetTarget(out _hooks);
        modLoader.GetController<IStartupScanner>().TryGetTarget(out _scanner);
    }

    internal static void RegisterPatterns(string patternsMod, string patternsFile)
    {
        try
        {
            var data = IniParsing.Instance.ReadFile(patternsFile);
            var patterns = data.Sections.FirstOrDefault(x => x.SectionName == Project.Id);
            if (patterns == null) return;

            foreach (var item in patterns.Keys)
            {
                Add(item.KeyName, item.Value);
                Log.Information($"Registered Pattern || Scan: {item.KeyName} || From: {patternsMod}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to register patterns from file.\nFile: {patternsFile}");
        }
    }

    /// <summary>
    /// Add a scan with the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="pattern">Scan pattern.</param>
    public static void Add(string scanId, string pattern)
    {
        _scanner!.Scan(scanId, pattern, result => OnScanSuccess(scanId, pattern, result), () => OnScanFailure(scanId, pattern));
        _scanPatterns[scanId] = pattern;
    }

    /// <summary>
    /// Add a scan with the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="pattern">Pattern to scan for.</param>
    /// <param name="onSuccess">Callback given result and hooks, on success.</param>
    public static void Add(string scanId, string pattern, Action<IReloadedHooks, nint> onSuccess)
    {
        Add(scanId, pattern);
        _listeners.Add(new ScanListener(scanId, result => onSuccess(_hooks!, result)));
    }

    /// <summary>
    /// Add a listener for the given Scan ID.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="success">Success action with result.</param>
    public static void Listen(string scanId, Action<IReloadedHooks, nint> success) => _listeners.Add(new ScanListener(scanId, result => success(_hooks!, result)));

    private static void OnScanSuccess(string scanId, string pattern, nint result)
    {
        // Ignore if scan was not for the latest pattern of Scan ID.
        if (_scanPatterns[scanId] != pattern) return;

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
        if (_scanPatterns[scanId] == pattern) Log.Error($"Failed to find pattern for \"{scanId}\". Pattern: {pattern}");
    }

    private record ScanListener(string ScanId, Action<nint> OnSuccess);
}