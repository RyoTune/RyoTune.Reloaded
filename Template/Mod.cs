//-:cnd:noEmit
#if DEBUG
using System.Diagnostics;
#endif
//+:cnd:noEmit
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded.Template.Template;
using RyoTune.Reloaded.Template.Configuration;

namespace RyoTune.Reloaded.Template;

public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly IReloadedHooks? _hooks;
    private readonly ILogger _log;
    private readonly IMod _owner;

    private Config _config;
    private readonly IModConfig _modConfig;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _log = context.Logger;
        _owner = context.Owner;
        _config = context.Configuration;
        _modConfig = context.ModConfig;
//-:cnd:noEmit
#if DEBUG
        Debugger.Launch();
#endif
//+:cnd:noEmit
        Project.Initialize(_modConfig, _modLoader, _log, true);
        Log.LogLevel = _config.LogLevel;
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _config = configuration;
        _log.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        Log.LogLevel = _config.LogLevel;
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}