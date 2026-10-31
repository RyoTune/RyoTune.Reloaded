namespace RyoTune.Reloaded.Common.Configs;

internal interface IConfigHandler
{
    bool CanHandle(string configId);

    void Handle(string configId, string newConfigFile);
}