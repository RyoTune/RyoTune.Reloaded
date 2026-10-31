using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Drawing;
using RyoTune.Reloaded.Common;
using RyoTune.Reloaded.Common.Configs;
using RyoTune.Reloaded.Scans;
using RyoTune.Reloaded.Serialization.Yaml.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace RyoTune.Reloaded;

/// <summary>
/// Core project functionality.
/// </summary>
public static class Project
{
    private static IModLoader _modLoader = null!;
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
    public static ISettings Settings { get; private set; } = null!;

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
    public static bool IsModDependent(IModConfigV1 mod) => mod.ModDependencies.Any(x => _dependableMods.Contains(x));

    private static void InitInternal(IModConfig modConfig, IModLoader modLoader)
    {
        _modLoader = modLoader;
        _dependableMods.Add(modConfig.ModId);
        
        Instance = new(_modLoader, modConfig);
        Scans = new ScansService(_modLoader);

        IYamlDeserializer? yamlDeserializer = null;

        if (modConfig.ModDependencies.Any(x => x == "RyoTune.Reloaded.Serialization.Yaml"))
            modLoader.GetController<IYamlDeserializer>().TryGetTarget(out yamlDeserializer);
        
        var configScans = new ScanConfigHandler(Scans) { YamlDeserializer = yamlDeserializer };
        var configSettings = new SettingsConfigHandler { YamlDeserializer = yamlDeserializer };
        Settings = configSettings;
        
        var configService = new ProjectConfigService(Instance);
        configService.AddHandler(configScans);
        configService.AddHandler(configSettings);
        _modRegisters.Add(configService);

        try { Directory.CreateDirectory(Instance.Directories.ProjectDirectory); }
        catch (Exception ex) { Log.Error(ex, "Failed to create project folder."); }

        _modLoader.ModLoaded += OnModLoaded;
    }

    private static void OnModLoaded(IModV1 mod, IModConfigV1 modConfig)
    {
        if (modConfig.ModDependencies.Any(x => _dependableMods.Contains(x)))
        {
            _dependableMods.Add(modConfig.ModId);
        }

        if (!IsModDependent(modConfig)) return;

        var modDir = _modLoader.GetDirectoryForModId(modConfig.ModId);
        foreach (var register in _modRegisters) register.RegisterMod(modConfig);
    }
}
