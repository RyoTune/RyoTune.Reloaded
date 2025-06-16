using Reloaded.Mod.Interfaces;

namespace RyoTune.Reloaded.Common;

/// <summary>
/// Mod project instance.
/// </summary>
public class Project
{
    /// <summary/>
    /// <param name="modLoader"></param>
    /// <param name="modConfig"></param>
    public Project(IModLoader modLoader, IModConfig modConfig)
    {
        Id = modConfig.ModId;
        Name = modConfig.ModName;
        ModDir = modLoader.GetDirectoryForModId(Id);
        ProjectDir = GetProjectFolder(ModDir);
        AppId = modLoader.GetAppConfig().AppId;
    }
    
    /// <summary>
    /// Project ID, equivalent to <c>ModId</c>.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Project name, equivalent to <c>ModName</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Project folder within mod folder.
    /// </summary>
    public string ProjectDir { get; }

    /// <summary>
    /// Project mod folder.
    /// </summary>
    public string ModDir { get; }

    /// <summary>
    /// Current application's ID.
    /// </summary>
    public string AppId { get; }

    /// <summary>
    /// Gets the expected project folder path within the base directory.
    /// </summary>
    /// <param name="baseDir">Base directory.</param>
    /// <returns>Project folder within base directory.</returns>
    public string GetProjectFolder(string baseDir) => Path.Join(baseDir, "Project", Id);
}