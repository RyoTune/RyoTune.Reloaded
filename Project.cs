using Reloaded.Mod.Interfaces;
using System.Drawing;

namespace RyoTune.Reloaded;

public static class Project
{
    private static IModLoader? _modLoader;

    public static void Init(IModConfig modConfig, IModLoader modLoader, ILogger log, bool useAsyncLog = false)
    {
        Log.Init(modConfig.ModId, log, useAsyncLog);
        _modLoader = modLoader;
    }

    public static void Init(IModConfig modConfig, IModLoader modLoader, ILogger log, Color color, bool useAsyncLog = false)
    {
        Log.Init(modConfig.ModId, log, color, useAsyncLog);
        _modLoader = modLoader;
    }

    public static void Start()
    {
        if (_modLoader != null)
        {
            ScanHooks.Init(_modLoader);
        }
    }
}
