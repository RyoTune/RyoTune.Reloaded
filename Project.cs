using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Drawing;

namespace RyoTune.Reloaded;

/// <summary>
/// Core project functionality.
/// </summary>
public static class Project
{
    private static IModLoader? _modLoader;

    internal static string? Id { get; private set; }

    internal static string? Name { get; private set; }

    internal static string? Folder { get; private set; }

    /// <summary>
    /// Initialize project functionality.
    /// </summary>
    /// <param name="modConfig">Mod config.</param>
    /// <param name="modLoader">Mod loader.</param>
    /// <param name="log">Logger.</param>
    /// <param name="useAsyncLog">Whether to use async logging.</param>
    public static void Initialize(IModConfig modConfig, IModLoader modLoader, ILogger log, bool useAsyncLog = false)
    {
        Log.Init(modConfig.ModId, log, useAsyncLog);
        InitInternal(modConfig, modLoader);
    }

    /// <summary>
    /// Initialize project functionality.
    /// </summary>
    /// <param name="modConfig">Mod config.</param>
    /// <param name="modLoader">Mod loader.</param>
    /// <param name="color">Predefined log color for info logs.</param>
    /// <param name="log">Logger.</param>
    /// <param name="useAsyncLog">Whether to use async logging.</param>
    public static void Initialize(IModConfig modConfig, IModLoader modLoader, ILogger log, Color color, bool useAsyncLog = false)
    {
        Log.Init(modConfig.ModId, log, color, useAsyncLog);
        InitInternal(modConfig, modLoader);
    }

    private static void InitInternal(IModConfig modConfig, IModLoader modLoader)
    {
        _modLoader = modLoader;

        Id = modConfig.ModId;
        Name = modConfig.ModName;
        Folder = Path.Join(modLoader.GetDirectoryForModId(modConfig.ModId), "Project");

        ScanHooks.Initialize(modLoader);
        modLoader.ModLoaded += OnModLoaded;
    }

    private static void OnModLoaded(IModV1 mod, IModConfigV1 config)
    {
        if (_modLoader == null || Id == null || !config.ModDependencies.Contains(Id)) return;

        var modDir = _modLoader.GetDirectoryForModId(config.ModId);
        var projectDir = Path.Join(modDir, "Project");

        var patternsFile = Path.Join(projectDir, ScanHooks.PATTERNS_FILE);
        if (File.Exists(patternsFile)) ScanHooks.RegisterPatterns(config.ModName, patternsFile);
    }
}
