// ReSharper disable InconsistentNaming

using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace RyoTune.Reloaded;

/// <summary/>
public static class IModLoaderExtensions
{
    private static bool _hasSetup;
    private static HashSet<string> _pendingMods = [];
    
    private static Action? OnModsLoaded { get; set; }
    
    /// <summary>
    /// Executes a callback once all mods have been loaded, but before <see cref="IModLoaderV1.OnModLoaderInitialized"/> is raised.<br/>
    /// Mainly a workaround for <see cref="IStartupScanner"/> no longer registering scans post-initialization.
    /// </summary>
    public static void WhenModsLoaded(this IModLoader modLoader, Action callback)
    {
        if (!_hasSetup)
        {
            Setup(modLoader);
            _hasSetup = true;
        }
        
        OnModsLoaded += callback;
    }

    private static void Setup(IModLoader modLoader)
    {
        var enabledMods = modLoader.GetAppConfig().EnabledMods;
        var activeMods = modLoader.GetActiveMods().Select(x => x.Generic.ModId);
        _pendingMods = enabledMods.Except(activeMods).ToHashSet();

        modLoader.ModLoaded += OnModLoaded;
    }

    private static void OnModLoaded(IModV1 _, IModConfigV1 modConfig)
    {
        _pendingMods.Remove(modConfig.ModId);
        if (_pendingMods.Count == 0) OnModsLoaded?.Invoke();
    }
}