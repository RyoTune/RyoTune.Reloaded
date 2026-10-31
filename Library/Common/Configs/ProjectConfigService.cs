using Reloaded.Mod.Interfaces.Internal;

namespace RyoTune.Reloaded.Common.Configs;

internal class ProjectConfigService(Project project) : IRegisterMod
{
    private readonly List<IConfigHandler> _handlers = [];

    public void RegisterMod(IModConfigV1 mod)
    {
        var dirs = project.GetProjectDirectoriesInMod(mod);
        if (!Directory.Exists(dirs.ProjectDirectory)) return;

        foreach (var configFile in Directory.EnumerateFiles(dirs.ProjectDirectory, "*"))
        {
            var configId = Path.GetFileNameWithoutExtension(configFile);
            var handler = _handlers.First(x => x.CanHandle(configId));
            handler.Handle(configId, configFile);
        }

        if (!Directory.Exists(dirs.ProjectAppDirectory)) return;
        foreach (var configFile in Directory.EnumerateFiles(dirs.ProjectAppDirectory, "*"))
        {
            var configId = Path.GetFileNameWithoutExtension(configFile);
            var handler = _handlers.First(x => x.CanHandle(configId));
            handler.Handle(configId, configFile);
        }
    }
    
    public void AddHandler(IConfigHandler handler) => _handlers.Add(handler);
}