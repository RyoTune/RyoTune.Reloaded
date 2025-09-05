// ReSharper disable InconsistentNaming

using System.Text;
using NCalc;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Inis;

namespace RyoTune.Reloaded.Scans;

/// <summary>
/// Scans service.
/// </summary>
public class ScansService : IScans
{
    private const string ScansIniId = "scans";
    private const string ScansIniSection = "Scans";
    private const string ResultSettingTag = "_RESULT";
    private const string DisabledScanValue = "DISABLED";
    
    private readonly IIni _inis;
    private readonly IModLoader _modLoader;
    private readonly IStartupScanner _scanner;
    private readonly IReloadedHooks _hooks;
    
    private readonly HashSet<Scan> _activeScans = [];
    private readonly Dictionary<string, string?> _latestPatterns = [];
    private readonly Dictionary<string, List<ScanListener>> _scanListeners = [];

    /// <summary>
    /// Scans service.
    /// </summary>
    /// <param name="inis">INI service.</param>
    /// <param name="modLoader">Mod loader.</param>
    public ScansService(IIni inis, IModLoader modLoader)
    {
        _inis = inis;
        _modLoader = modLoader;
        _modLoader.GetController<IStartupScanner>().TryGetTarget(out _scanner!);
        _modLoader.GetController<IReloadedHooks>().TryGetTarget(out _hooks!);
        
        _inis.UsingSettings(ScansIniId, ScansIniSection, settings =>
        {
            foreach (var setting in settings)
            {
                if (setting.Key.EndsWith(ResultSettingTag)) continue;
                AddScan(setting.Key, setting.Value, false);
            }
        });

        _modLoader.OnModLoaderInitialized += () =>
        {
            var templateFile = Path.Join(Project.Instance.ProjectDir, $"{ScansIniId}-template.txt");
#if RELEASE
            if (File.Exists(templateFile)) return;
#endif
            var sb = new StringBuilder();
            sb.Append($"[{ScansIniSection}]");
            foreach (var scanId in _scanListeners.Keys)
            {
                sb.AppendLine();
                sb.AppendLine($";{scanId}=");
                sb.AppendLine($";{GetExprName(scanId)}=");
            }
            
            File.WriteAllText(templateFile, sb.ToString());
        };
    }

    /// <inheritdoc />
    public void AddScan(string id, Action<nint> onSuccess, Action? onFail = null)
        => AddScan(id, null, onSuccess, onFail);

    /// <inheritdoc />
    public void AddScanHook(string id, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => AddScan(id, result => onSuccess(result, _hooks), onFail);

    /// <inheritdoc />
    public void AddScan(string id, string? pattern, Action<nint> onSuccess, Action? onFail = null)
    {
        AddListener(id, onSuccess, onFail);
        AddScan(id, pattern);
    }

    /// <inheritdoc />
    public void AddScanHook(string id, string? pattern, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => AddScan(id, pattern, result => onSuccess(result, _hooks), onFail);

    /// <inheritdoc />
    public void AddScan(string id, nint defaultResult, Action<nint> onSuccess, Action? onFail = null)
    {
        AddScan(id, onSuccess, onFail);

        var scan = new Scan(id, null);
        if (_activeScans.Contains(scan)) return;
        
        _modLoader.OnModLoaderInitialized += () =>
        {
            // If no scan pattern was ever provided, invoke scan
            // as successful with the provided default result.
            if (_latestPatterns.ContainsKey(id)) return;
            if (!_scanListeners.TryGetValue(id, out var listeners)) return;
            foreach (var listener in listeners) listener.OnSuccess(defaultResult);
        };

        _activeScans.Add(scan);
    }

    /// <inheritdoc />
    public void AddScanHook(string id, nint defaultResult, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => AddScan(id, defaultResult, result => onSuccess(result, _hooks), onFail);

    /// <inheritdoc />
    public void AddListener(string id, Action<nint> onSuccess, Action? onFail = null)
    {
        // Add listener for scan.
        if (!_scanListeners.ContainsKey(id)) _scanListeners[id] = [];
        _scanListeners[id].Add(new(onSuccess, onFail));
    }

    /// <summary>
    /// Add a scan.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="pattern">Initial scan pattern, if any.</param>
    /// <param name="useLocal">Whether the pattern can be overwritten by a local <b>Scan INI</b>.</param>
    private void AddScan(string id, string? pattern, bool useLocal = true)
    {
        // Override pattern with one provided through a mod Scan INI, if any.
        if (useLocal && _inis.TryGetSetting(ScansIniId, id, ScansIniSection, out var newPattern)) pattern = newPattern;
        if (pattern == null) return;
        
        _latestPatterns[id] = pattern;
        if (pattern == DisabledScanValue) return;
        
        // Add new scan to scanner.
        var scan = new Scan(id, pattern);
        
        // Ensure only one scan per pattern so listeners only receive a single reply/run once.
        // Besides redundancy, this prevents the risk of multiple hooks being created for the
        // same scan. Since they'll likely be saved to the same field, older hooks will end up GC'd and cause crashes.
        if (_activeScans.Contains(scan)) return;
        
        _scanner.Scan(id, pattern, result => OnScanSuccess(id, pattern, result), () => OnScanFailure(id, pattern));
        _activeScans.Add(scan);
    }

    /// <summary>
    /// Successful scan callback.<br/>
    /// This callback ensures only the scan with the latest pattern responds to scan listeners.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="scanPattern">Scan pattern this callback was made with.</param>
    /// <param name="result">The scan result.</param>
    private unsafe void OnScanSuccess(string scanId, string scanPattern, nint result)
    {
        // Ignore any callback made not using the latest scan pattern.
        if (scanPattern != _latestPatterns[scanId]) return;

        var exprName = $"{GetExprName(scanId)}";
        if (_inis.TryGetSetting(ScansIniId, exprName, ScansIniSection, out var exprStr))
        {
            var expr = new Expression(exprStr)
            {
                Parameters =
                {
                    ["result"] = (long)result
                },
                Functions =
                {
                    ["GetGlobalAddress"] = args => (long)Utilities.GetGlobalAddress((int*)(long)args[0].Evaluate()!)
                }
            };

            if (expr.HasErrors())
            {
                Log.Error($"Scan '{scanId}' result expression contained error(s).\nExpression: {exprStr}\nError: {expr.Error}");
                return;
            }

            try
            {
                var exprValue = (long?)expr.Evaluate() ?? throw new();
                result = (nint)exprValue;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to evaluate scan '{scanId}' result expression.");
                return;
            }
        }
        
        Log.Information($"'{scanId}' found at: 0x{result:X}");
        if (!_scanListeners.TryGetValue(scanId, out var listeners)) return;
        foreach (var listener in listeners) listener.OnSuccess(result);
    }
    
    /// <summary>
    /// Failed scan callback.<br/>
    /// This callback ensures only the scan with the latest pattern responds to scan listeners.
    /// </summary>
    /// <param name="scanId">Scan ID.</param>
    /// <param name="scanPattern">Scan pattern this callback was made with.</param>
    private void OnScanFailure(string scanId, string scanPattern)
    {
        // Ignore any callback made not using the latest scan pattern.
        if (scanPattern != _latestPatterns[scanId]) return;

        if (!_scanListeners.TryGetValue(scanId, out var listeners)) return;
        foreach (var listener in listeners)
        {
            if (listener.OnFailure != null)
            {
                listener.OnFailure();
            }
            else
            {
                Log.Error($"Failed to find pattern for '{scanId}'.\nPattern: {scanPattern}");
            }
        }
    }

    private static string GetExprName(string id) => $"{id}{ResultSettingTag}";
    
    private readonly record struct Scan(string Id, string? Pattern);
    
    private readonly record struct ScanListener(Action<nint> OnSuccess, Action? OnFailure);
}