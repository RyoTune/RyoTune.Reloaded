namespace RyoTune.Reloaded.Common;

/// <summary>
/// Interface for services that register items from other mods.
/// </summary>
public interface IRegisterMod
{
    /// <summary>
    /// Register mod.
    /// </summary>
    /// <param name="modId">Mod ID.</param>
    /// <param name="modName">Mod name.</param>
    /// <param name="modDir">Mod directory.</param>
    void RegisterMod(string modId, string modName, string modDir);
}