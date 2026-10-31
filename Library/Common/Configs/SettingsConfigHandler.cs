using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using IniParser;
using RyoTune.Reloaded.Serialization.Yaml.Interfaces;

namespace RyoTune.Reloaded.Common.Configs;

internal class SettingsConfigHandler : ISettings, IConfigHandler
{
    public static readonly FileIniDataParser IniParser = new();
    private const string DefaultSectionId = "default";
    
    private readonly Dictionary<string, ConfigSettings> _configs = [];
    private Action<ConfigChangedArgs>? _configChanged;
    private Action<SettingChangedArgs>? _settingChanged;

    public SettingsConfigHandler()
    {
        _configChanged += args =>
        {
            foreach (var setting in args.Settings)
                _settingChanged?.Invoke(new(args.ConfigId, setting.Key, args.Section, setting.Value));
        };
    }

    /// <inheritdoc />
    public bool CanHandle(string configId) => true;

    public IYamlDeserializer? YamlDeserializer { get; set; }

    /// <inheritdoc />
    public void Handle(string configId, string newConfigFile)
    {
        var ext = Path.GetExtension(newConfigFile).ToLowerInvariant();

        ConfigSettings? newSettings = null;
        
        if (ext == ".ini")
        {
            var iniData = IniParser.ReadFile(newConfigFile);
            newSettings = new(iniData.Sections.ToDictionary(x => x.SectionName, x => x.Keys.ToDictionary(y => y.KeyName, y => y.Value)));
            foreach (var setting in iniData.Global)
                newSettings[DefaultSectionId][setting.KeyName] = setting.Value;
        }
        else if (YamlDeserializer != null && ext == ".yaml")
        {
            newSettings = YamlDeserializer.Deserialize<ConfigSettings>(File.ReadAllText(newConfigFile));
        }
        
        if (newSettings == null) return;
        
        if (!_configs.TryAdd(configId, newSettings)) _configs[configId].Merge(newSettings);
        foreach (var (section, settings) in newSettings)
            _configChanged?.Invoke(new(configId, section, settings));
    }

    /// <inheritdoc />
    public bool TryGetSetting(string configId, string name, [NotNullWhen(true)] out string? value)
        => TryGetSetting(configId, name, null, out value);
    
    /// <inheritdoc />
    public bool TryGetSetting(string configId, string name, string? section, [NotNullWhen(true)] out string? value)
    {
        value = GetSetting(configId, name, section);
        return value != null;
    }
    
    /// <inheritdoc />
    public bool TryGetSetting<TValue>(string configId, string name, [NotNullWhen(true)] out TValue? value)
        => TryGetSetting(configId, name, null, out value);
    
    /// <inheritdoc />
    public bool TryGetSetting<TValue>(string configId, string name, string? section, [NotNullWhen(true)] out TValue? value)
    {
        var valueStr = GetSetting(configId, name, section);
        if (valueStr == null)
        {
            value = default;
            return false;
        }
        
        value = ResolveValue<TValue>(valueStr)!;
        return true;
    }
    
    /// <inheritdoc />
    public string? GetSetting(string configId, string name, string? section = null)
    {
        section ??= DefaultSectionId;
        
        if (_configs.TryGetValue(configId, out var config)
            && config.TryGetValue(section, out var settings)
            && settings.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }

    /// <inheritdoc />
    public void UsingSetting(string configId, string name, string? section, Action<string> newValue)
    {
        // Send initial value.
        if (TryGetSetting(configId, name, section, out var value)) newValue(value);
        _settingChanged += args =>
        {
            if (args.ConfigId == configId && args.Name == name && args.Section == section) newValue(args.Value);
        };
    }

    /// <inheritdoc />
    public void UsingSetting<TValue>(string configId, string name, string? section, Action<TValue> newValue)
    {
        // Send initial value.
        if (TryGetSetting<TValue>(configId, name, section, out var value)) newValue(value);
        _settingChanged += args =>
        {
            if (args.ConfigId == configId && args.Name == name && args.Section == section) newValue(ResolveValue<TValue>(args.Value));
        };
    }

    /// <inheritdoc />
    public void UsingSettings(string configId, string? section, Action<IReadOnlyDictionary<string, string>> newValues)
    {
        // Send initial value.
        if (section == null) return;
        if (_configs.TryGetValue(configId, out var config)) newValues(config[section]);
        _configChanged += args =>
        {
            if (args.ConfigId == configId && args.Section == section) newValues(args.Settings);
        };
    }
    
    private static TValue ResolveValue<TValue>(string value)
    {
        // Handle hex integer values.
        if (value.StartsWith("0x"))
        {
            var integerValue = Convert.ToUInt64(value, 16);
            return (TValue)Convert.ChangeType(integerValue, typeof(TValue), CultureInfo.InvariantCulture);
        }

        return (TValue)Convert.ChangeType(value, typeof(TValue), CultureInfo.InvariantCulture);
    }
    
    private record SettingChangedArgs(string ConfigId, string Name, string? Section, string Value);

    private record ConfigChangedArgs(string ConfigId, string? Section, IReadOnlyDictionary<string, string> Settings);
    
    private class ConfigSettings : Dictionary<string, Dictionary<string, string>>
    {
        public ConfigSettings()
        {
        }
        
        public ConfigSettings(IDictionary<string, Dictionary<string, string>> dictionary) : base(dictionary)
        {
        }
        
        public void Merge(ConfigSettings newSettings)
        {
            foreach (var (section, settings) in newSettings)
            {
                if (!ContainsKey(section))
                    Add(section, settings);
                else
                    foreach (var (name, value) in settings)
                        this[section][name] = value;
            }
        }
    }
}
