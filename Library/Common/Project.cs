using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace RyoTune.Reloaded.Common;

/// <summary>
/// Mod project instance.
/// </summary>
public class Project
{
    private readonly IModLoader _modLoader;

    /// <summary/>
    public Project(IModLoader modLoader, IModConfig projectMod)
    {
        _modLoader = modLoader;
        
        AppId = Path.GetFileNameWithoutExtension(modLoader.GetAppConfig().AppId);
        Id = projectMod.ModId;
        Name = projectMod.ModName;
        ModDir = modLoader.GetDirectoryForModId(Id);
        
        Directories = GetProjectDirectoriesInMod(projectMod);
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
    /// Project directories.
    /// </summary>
    public ProjectDirectories Directories { get; }

    /// <summary>
    /// Project mod folder.
    /// </summary>
    public string ModDir { get; }

    /// <summary>
    /// Current application's ID.
    /// </summary>
    public string AppId { get; }

    /// <summary>
    /// Get this project's folders within a mod.
    /// </summary>
    public ProjectDirectories GetProjectDirectoriesInMod(IModConfigV1 mod)
    {
        var modDir = _modLoader.GetDirectoryForModId(mod.ModId);
        var projectDir = Path.Join(modDir, "Project", Id);
        var projectAppDir = Path.Join(projectDir, AppId);
        return new(projectDir, projectAppDir);
    }

    public record ProjectDirectories(string ProjectDirectory, string ProjectAppDirectory);
}