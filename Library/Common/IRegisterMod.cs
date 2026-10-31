using Reloaded.Mod.Interfaces.Internal;

namespace RyoTune.Reloaded.Common;

/// <summary>
/// Interface for services that register items from other mods.
/// </summary>
public interface IRegisterMod
{
    /// <summary>
    /// Register a mod.
    /// </summary>
    /// <param name="mod">The mod to register.</param>
    void RegisterMod(IModConfigV1 mod);
}