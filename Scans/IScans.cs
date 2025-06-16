using Reloaded.Hooks.Definitions;

namespace RyoTune.Reloaded.Scans;

/// <summary>
/// Scans service interface.
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
    /// <param name="onSuccess">Callback given scan result and hooks, if successful.</param>
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
    /// <param name="onSuccess">Callback given scan result and hooks, if successful.</param>
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
    /// Add a scan listener.
    /// </summary>
    /// <param name="id">Scan ID.</param>
    /// <param name="onSuccess">Callback given scan result, if successful.</param>
    /// <param name="onFail">Callback on scan failure.</param>
    void AddListener(string id, Action<nint> onSuccess, Action? onFail = null);
}