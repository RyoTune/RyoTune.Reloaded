using Reloaded.Hooks.Definitions;
using RyoTune.Reloaded.Scans.Models;

namespace RyoTune.Reloaded.Scans;

/// <summary>
/// Scans service.
/// </summary>
public interface IScans
{
    /// <summary>
    /// Add a scan.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="onSuccess">Callback given scan result, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    void AddScan(string id, Action<nint> onSuccess, Action? onFail = null);
    
    /// <summary>
    /// Add a scan.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="onSuccess">Callback given a scan result and hooks, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    void AddScanHook(string id, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null);

    /// <summary>
    /// Add a scan.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="pattern">Initial scan pattern, if any.</param>
    /// <param name="onSuccess">Callback given scan result, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    void AddScan(string id, string? pattern, Action<nint> onSuccess, Action? onFail = null);

    /// <summary>
    /// Add a scan.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="pattern">Initial scan pattern, if any.</param>
    /// <param name="onSuccess">Callback given a scan result and hooks, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    void AddScanHook(string id, string? pattern, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null);

    /// <summary>
    /// Add a scan with a default result.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="defaultResult">Default result.</param>
    /// <param name="onSuccess">Callback given scan result, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    /// <remarks>Allows for using hardcoded addresses during mod development, that can later be replaced with scan patterns without code changes.</remarks>
    void AddScan(string id, nint defaultResult, Action<nint> onSuccess, Action? onFail = null);

    /// <summary>
    /// Add a scan with a default result.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="defaultResult">Default result.</param>
    /// <param name="onSuccess">Callback given scan result, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    /// <remarks>Allows for using hardcoded addresses during mod development, that can later be replaced with scan patterns without code changes.</remarks>
    void AddScanHook(string id, nint defaultResult, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null);

    /// <summary>
    /// Add a scan candidate.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="pattern">Scan pattern.</param>
    void AddScanCandidate(string id, string pattern);
    
    /// <summary>
    /// Add a scan candidate.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="candidate">Scan candidate.</param>
    void AddScanCandidate(string id, ScanCandidate candidate);

    /// <summary>
    /// Reset all scan candidates.
    /// </summary>
    /// <param name="id">ID of scan to reset.</param>
    void ResetScanCandidates(string id);
    
    /// <summary>
    /// Remove a scan.
    /// </summary>
    /// <param name="id">ID of scan to remove.</param>
    void RemoveScan(string id);
    
    /// <summary>
    /// Set whether a scan is required.
    /// </summary>
    /// <param name="id">ID of a scan to set.</param>
    /// <param name="required">Whether an error will be logged if the scan fails.</param>
    void SetScanRequired(string id, bool required);

    /// <summary>
    /// Whether a scan exists.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    bool HasScan(string id);
}