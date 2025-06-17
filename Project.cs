using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Drawing;
using RyoTune.Reloaded.Common;
using RyoTune.Reloaded.Inis;
using RyoTune.Reloaded.Scans;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace RyoTune.Reloaded;

/// <summary>
/// Core project functionality.
/// </summary>
public static class Project
{
    private static IModLoader? _modLoader;
    private static readonly List<IRegisterMod> _modRegisters = [];
    private static readonly HashSet<string> _dependableMods = [];

    /// <summary>
    /// Project instance.
    /// </summary>
    public static Common.Project Instance { get; private set; } = null!;

    /// <summary>
    /// Project scans service.
    /// </summary>
    public static IScans Scans { get; private set; } = null!;

    /// <summary>
    /// Project INI service.
    /// </summary>
    public static IIni Inis { get; private set; } = null!;

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

    /// <summary>
    /// Checks whether the given mod is dependent on this mod, including as a sub-dependency.<br/>
    /// Example: Given <c>Mod A &lt;- Mod B &lt;- Mod C</c>, <c>Mod C</c> would be considered dependent on <c>Mod A</c>.
    /// </summary>
    /// <param name="mod">Mod to check.</param>
    /// <returns>Whether the given mod is dependent on this mod.</returns>
    public static bool IsModDependant(IModConfigV1 mod) => mod.ModDependencies.Any(x => _dependableMods.Contains(x));

    private static void InitInternal(IModConfig modConfig, IModLoader modLoader)
    {
        _modLoader = modLoader;
        _dependableMods.Add(modConfig.ModId);
        
        Instance = new(_modLoader, modConfig);
        
        Inis = new IniService(Instance);
        _modRegisters.Add(Inis);
        
        Scans = new ScansService(Inis, _modLoader);

        try { Directory.CreateDirectory(Instance.ProjectDir); }
        catch (Exception ex) { Log.Error(ex, "Failed to create project folder."); }

        _modLoader.ModLoaded += OnModLoaded;
    }

    private static void OnModLoaded(IModV1 mod, IModConfigV1 modConfig)
    {
        if (modConfig.ModDependencies.Any(x => _dependableMods.Contains(x)))
        {
            _dependableMods.Add(modConfig.ModId);
        }
        
        if (_modLoader == null || !modConfig.ModDependencies.Contains(Instance.Id)) return;

        var modDir = _modLoader.GetDirectoryForModId(modConfig.ModId);
        foreach (var register in _modRegisters) register.RegisterMod(modConfig.ModId, modConfig.ModName, modDir);
    }
}
