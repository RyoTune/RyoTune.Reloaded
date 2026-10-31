using NCalc;
using NCalc.Handlers;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RyoTune.Reloaded.Scans.Models;
using RyoTune.Reloaded.Utilities;

namespace RyoTune.Reloaded.Scans;

/// <inheritdoc />
public class ScansService : IScans
{
    private readonly Dictionary<string, ExpressionFunction> _transFunctions = new()
    {
        ["ResolveRelativeAddress"] = args => (long)RelativeAddress.Resolve((nint)(long)args.Evaluate(0)!),
#pragma warning disable CS0618 // Backwards compatibility with existing scans.
        ["GetGlobalAddress"] = args => (long)RelativeAddress.GetGlobalAddress((nint)(long)args.Evaluate(0)!)
#pragma warning restore CS0618
    };
    
    private readonly IStartupScanner _scanner;
    private readonly IReloadedHooks _hooks;

    private readonly Dictionary<string, Scan> _scans = [];
    private readonly ScanFilter _scanFilter;
    private int _numTotalCandidates;
    private int _numCompletedCandidates;

    private readonly int _numTotalMods;
    private int _numLoadedMods;
    
    /// <summary/>
    public ScansService(IModLoader modLoader)
    {
        _numTotalMods = modLoader.GetAppConfig().EnabledMods.Length;
        _scanFilter = new(modLoader.GetAppConfig().AppLocation);
        modLoader.GetController<IStartupScanner>().TryGetTarget(out _scanner!);
        modLoader.GetController<IReloadedHooks>().TryGetTarget(out _hooks!);
        modLoader.ModLoaded += OnModLoaded;
    }

    private void OnModLoaded(IModV1 _, IModConfigV1 __)
    {
        _numLoadedMods++;
        
        // Jank workaround for the fact IStartupScanner will not register
        // new scans after its OnModLoaderInitialized callback runs, which
        // will always be after any dependent mod...
        // So, we'll just register scans right after the last mod loads :)
        if (_numLoadedMods == _numTotalMods) RegisterScans();
    }
    
    private bool TryResolveResultValue(ScanCandidateResult result, out nint value)
    {
        if (result.Candidate.Transform == null)
        {
            value = result.Value;
            return true;
        }

        var expr = new Expression(result.Candidate.Transform, new ExpressionContext
        {
            Parameters = { { "result", (long)result.Value } },
            Functions = _transFunctions,
        });

        value = 0;
        var identifier = result.Candidate.Name ?? result.Candidate.Pattern;
        if (expr.HasErrors())
        {
            Log.Error($"Scan '{identifier}' transformation contained error(s).\n" +
                      $"Transform: {result.Candidate.Transform}\n" +
                      $"Error: {expr.Error}");
            return false;
        }
        
        try
        {
            var exprValue = (nint?)(long?)expr.Evaluate() ?? throw new();
            value = exprValue;
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to evaluate scan '{identifier}' result expression.");
            return false;
        }
    }

    private void BroadcastScans()
    {
        foreach (var (id, scan) in _scans)
        {
            var result = scan.Results
                .OrderBy(x => x.Order)
                .FirstOrDefault(x => x.IsFound);

            if (result == null && scan.TryGetDefault(out var defaultResult))
                result = new(0, new() { Name = "Default" }, true, defaultResult);

            if (result != null && TryResolveResultValue(result, out var value))
            {
                var withMsg = result.Candidate.Name != null
                    ? $" with '{result.Candidate.Name}'"
                    : string.Empty;
                
                Log.Information($"'{id}' found{withMsg} at: 0x{value:X}");
                foreach (var receiver in scan.Receivers) receiver.OnSuccess(value);
            }
            else
            {
                if (scan.IsRequired)
                {
                    Log.Error(
                        $"'{id}' was not found with any scan. Candidates:\n" +
                        string.Join("\n- ", scan.Candidates.Select(x => x.Name ?? x.Pattern)));
                }
                
                foreach (var receiver in scan.Receivers) receiver.OnFailure?.Invoke();
            }
        }
    }
    
    private void RegisterScans()
    {
        foreach (var (scanId, scan) in _scans)
        {
            for (var index = 0; index < scan.Candidates.Count; index++)
            {
                var candidate = scan.Candidates[index];
                if (candidate.Pattern == null) continue;

                var order = index;
                _scanner.AddMainModuleScan(candidate.Pattern, result =>
                {
                    var scanResult = new ScanCandidateResult(
                        order,
                        candidate,
                        result.Found,
                        IStartupScannerExtensions.BaseAddress + result.Offset);

                    scan.SetCandidateResult(scanResult);
                    
                    if (Interlocked.Increment(ref _numCompletedCandidates) == _numTotalCandidates)
                        BroadcastScans();
                });

                _numTotalCandidates++;
            }
        }
    }

    /// <inheritdoc />
    public void AddScan(string id, Action<nint> onSuccess, Action? onFail = null)
    {
        var scan = CreateOrGetScan(id);
        scan.AddReceiver(onSuccess, onFail);
    }

    /// <inheritdoc />
    public void AddScanHook(string id, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => AddScan(id, result => onSuccess(result, _hooks), onFail);

    /// <inheritdoc />
    public void AddScan(string id, string? pattern, Action<nint> onSuccess, Action? onFail = null)
    {
        var scan = CreateOrGetScan(id);
        scan.AddCandidate(new() { Pattern = pattern });
        scan.AddReceiver(onSuccess, onFail);
    }

    /// <inheritdoc />
    public void AddScanHook(string id, string? pattern, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => AddScan(id, pattern, result => onSuccess(result, _hooks), onFail);

    /// <inheritdoc />
    public void AddScan(string id, nint defaultResult, Action<nint> onSuccess, Action? onFail = null)
    {
        var scan = CreateOrGetScan(id);
        scan.SetDefault(defaultResult);
        scan.AddReceiver(onSuccess, onFail);
    }

    /// <inheritdoc />
    public void AddScanHook(string id, nint defaultResult, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null)
        => AddScan(id, defaultResult, result => onSuccess(result, _hooks), onFail);

    /// <inheritdoc />
    public void AddScanCandidate(string id, string pattern)
    {
        var scan = CreateOrGetScan(id);
        scan.AddCandidate(new() { Pattern = pattern });
    }

    /// <inheritdoc />
    public void AddScanCandidate(string id, ScanCandidate candidate)
    {
        var scan = CreateOrGetScan(id);
        scan.AddCandidate(candidate);
    }

    /// <inheritdoc />
    public void RemoveScan(string id) => _scans.Remove(id);

    /// <inheritdoc />
    public void ResetScanCandidates(string id)
    {
        var scan = CreateOrGetScan(id);
        scan.ResetCandidates();
    }

    /// <inheritdoc />
    public void SetScanRequired(string id, bool required)
    {
        var scan = CreateOrGetScan(id);
        scan.IsRequired = required;
    }

    private Scan CreateOrGetScan(string id)
    {
        if (_scans.TryGetValue(id, out var scan)) return scan;
        _scans[id] = new(_scanFilter);
        return _scans[id];
    }
}