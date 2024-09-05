using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace RyoTune.Reloaded;

public static class ScanHooks
{
    private static readonly List<ScanHook> scans = [];
    private static readonly List<ScanListener> listeners = [];

    /// <summary>
    /// Add a new a scan hook.
    /// </summary>
    /// <param name="name">Name of scan.</param>
    /// <param name="pattern">Pattern to scan for.</param>
    /// <param name="success">Success action with hooks and result.</param>
    public static void Add(string name, string? pattern, Action<IReloadedHooks, nint> success)
        => scans.Add(new(name, pattern, success));

    /// <summary>
    /// Add a listener for an existing scan. Listeners only need the name
    /// of the scan and are given the result and hooks if found.
    /// </summary>
    /// <param name="name">Name of scan.</param>
    /// <param name="success">Success action with result.</param>
    public static void Listen(string name, Action<IReloadedHooks, nint> success)
        => listeners.Add(new(name, success));

    public static void Init(IModLoader modLoader)
    {
        modLoader.GetController<IStartupScanner>().TryGetTarget(out var scanner);
        modLoader.GetController<IReloadedHooks>().TryGetTarget(out var hooks);
        Init(scanner!, hooks!);
    }

    public static void Init(IStartupScanner scanner, IReloadedHooks hooks)
    {
        foreach (var scan in scans)
        {
            if (string.IsNullOrEmpty(scan.Pattern))
            {
                Log.Verbose($"{scan.Name}: No pattern given.");
                continue;
            }

            scanner.Scan(scan.Name, scan.Pattern, result =>
            {
                scan.Success(hooks, result);
                foreach (var item in listeners.Where(x => x.Name == scan.Name).ToArray())
                {
                    item.Success.Invoke(hooks, result);
                }
            });
        }
    }

    private record ScanHook(string Name, string? Pattern, Action<IReloadedHooks, nint> Success);

    private record ScanListener(string Name, Action<IReloadedHooks, nint> Success);
}