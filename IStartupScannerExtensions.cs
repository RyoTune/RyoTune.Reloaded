using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace RyoTune.Reloaded;

/// <summary>
/// <see cref="IStartupScanner"/> extension methods.
/// </summary>
public static class IStartupScannerExtensions
{
    /// <summary>
    /// Basic pattern scan with logging and callback to execute with successful result.
    /// </summary>
    /// <param name="scanner">Scanner.</param>
    /// <param name="name">Scan name for logs.</param>
    /// <param name="pattern">Scan pattern.</param>
    /// <param name="onSuccess">Callback which is given the result on success.</param>
    public static void Scan(
        this IStartupScanner scanner,
        string name,
        string pattern,
        Action<nint> onSuccess)
    {
        scanner.AddMainModuleScan(pattern, result =>
        {
            if (!result.Found)
            {
                Log.Error($"Failed to find pattern for \"{name}\". Pattern: {pattern}");
                return;
            }

            var address = Utilities.BaseAddress + result.Offset;
            Log.Information($"\"{name}\" found at: 0x{address:X}");
            onSuccess(address);
        });
    }

    /// <summary>
    /// Pattern scan with no logging and success and failure callbacks, for more control.
    /// </summary>
    /// <param name="scanner">Scanner.</param>
    /// <param name="name">Scan name for logs.</param>
    /// <param name="pattern">Scan pattern.</param>
    /// <param name="onSuccess">Callback which is given the scan result on success.</param>
    /// <param name="onFailure">Callback which is run on scan failure.</param>
    public static void Scan(
        this IStartupScanner scanner,
        string name,
        string pattern,
        Action<nint> onSuccess,
        Action onFailure)
    {
        scanner.AddMainModuleScan(pattern, result =>
        {
            if (!result.Found)
            {
                onFailure();
                return;
            }

            var address = Utilities.BaseAddress + result.Offset;
            onSuccess(address);
        });
    }
}
