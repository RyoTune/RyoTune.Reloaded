using Reloaded.Mod.Interfaces;
using System.Drawing;

namespace RyoTune.Reloaded;

public static class Project
{
    public static void Init(IModConfig mod, IModLoader modLoader, ILogger log, bool useAsyncLog = false)
    {
        Log.Init(mod.ModId, log, useAsyncLog);
        ScanHooks.Init(modLoader);
    }

    public static void Init(IModConfig mod, IModLoader modLoader, ILogger log, Color color, bool useAsyncLog = false)
    {
        Log.Init(mod.ModId, log, color, useAsyncLog);
        ScanHooks.Init(modLoader);
    }
}
