using Reloaded.Mod.Interfaces;
using System.Drawing;

namespace RyoTune.Reloaded;

public static class Project
{
    public static void Init(IModConfig modConfig, IModLoader modLoader, ILogger log, bool useAsyncLog = false)
    {
        Log.Init(modConfig.ModId, log, useAsyncLog);
        ScanHooks.Init(modLoader);
    }

    public static void Init(IModConfig modConfig, IModLoader modLoader, ILogger log, Color color, bool useAsyncLog = false)
    {
        Log.Init(modConfig.ModId, log, color, useAsyncLog);
        ScanHooks.Init(modLoader);
    }
}
