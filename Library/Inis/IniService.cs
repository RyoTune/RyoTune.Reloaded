using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using IniParser;
using IniParser.Model;

namespace RyoTune.Reloaded.Inis;

/// <summary>
/// Service for getting settings from INI files, with support for game-specific
/// and external mod overrides.
/// </summary>
public class IniService : IIni
{
    private static readonly FileIniDataParser Parser = new();
    
    private readonly Common.Project _project;
    private readonly Dictionary<string, IniData> _inis = [];
    private Action<SettingChangedArgs>? _settingChanged;
    private Action<IniChangedArgs>? _iniChanged;

    /// <summary>
    /// INI service.
    /// </summary>
    /// <param name="project"></param>
    public IniService(Common.Project project)
    {
        _project = project;

        _iniChanged += args =>
        {
            foreach (var setting in args.Settings)
                _settingChanged?.Invoke(new(args.IniId, setting.Key, args.Section, setting.Value));
        };
        
        // Load own INI files.
        LoadModInis(project.ModDir);
    }

    /// <inheritdoc />
    public void RegisterMod(string modId, string modName, string modDir)
    {
        LoadModInis(modDir);
    }

    /// <inheritdoc />
    public bool TryGetSetting(string iniId, string name, [NotNullWhen(true)] out string? value)
        => TryGetSetting(iniId, name, null, out value);
    
    /// <inheritdoc />
    public bool TryGetSetting(string iniId, string name, string? section, [NotNullWhen(true)] out string? value)
    {
        value = GetSetting(iniId, name, section);
        return value != null;
    }
    
    /// <inheritdoc />
    public bool TryGetSetting<TValue>(string iniId, string name, [NotNullWhen(true)] out TValue? value)
        => TryGetSetting(iniId, name, null, out value);
    
    /// <inheritdoc />
    public bool TryGetSetting<TValue>(string iniId, string name, string? section, [NotNullWhen(true)] out TValue? value)
    {
        var valueStr = GetSetting(iniId, name, section);
        if (valueStr == null)
        {
            value = default;
            return false;
        }
        
        value = ResolveValue<TValue>(valueStr)!;
        return true;
    }
    
    /// <inheritdoc />
    public string? GetSetting(string iniId, string name, string? section = null)
    {
        if (_inis.TryGetValue(iniId, out var ini))
        {
            var key = section == null ? name : $"{section}.{name}";
            if (ini.TryGetKey(key, out var value)) return value;
        }

        return null;
    }

    /// <inheritdoc />
    public void UsingSetting(string iniId, string name, string? section, Action<string> newValue)
    {
        // Send initial value.
        if (TryGetSetting(iniId, name, section, out var value)) newValue(value);
        _settingChanged += args =>
        {
            if (args.IniId == iniId && args.Name == name && args.Section == section) newValue(args.Value);
        };
    }

    /// <inheritdoc />
    public void UsingSetting<TValue>(string iniId, string name, string? section, Action<TValue> newValue)
    {
        // Send initial value.
        if (TryGetSetting<TValue>(iniId, name, section, out var value)) newValue(value);
        _settingChanged += args =>
        {
            if (args.IniId == iniId && args.Name == name && args.Section == section) newValue(ResolveValue<TValue>(args.Value));
        };
    }

    /// <inheritdoc />
    public void UsingSettings(string iniId, string? section, Action<IReadOnlyDictionary<string, string>> newValues)
    {
        // Send initial value.
        if (_inis.TryGetValue(iniId, out var iniData)) newValues(iniData.ToSection(section));
        _iniChanged += args =>
        {
            if (args.IniId == iniId && args.Section == section) newValues(args.Settings);
        };
    }

    private void LoadModInis(string modDir)
    {
        // Load base INI files, located in project folder root.
        var projDir = _project.GetProjectFolder(modDir);
        if (!Directory.Exists(projDir)) return;
        
        foreach (var iniFile in Directory.EnumerateFiles(projDir, "*.ini", SearchOption.TopDirectoryOnly))
        {
            LoadIniFile(iniFile);
        }
        
        // Load and apply current app specific INI settings.
        var projAppDir = Path.Join(projDir, _project.AppId);
        if (!Directory.Exists(projAppDir)) return;
        
        foreach (var iniFile in Directory.EnumerateFiles(projAppDir, "*.ini", SearchOption.TopDirectoryOnly))
        {
            LoadIniFile(iniFile);
        }
    }

    private void LoadIniFile(string iniFile)
    {
        var iniId = Path.GetFileNameWithoutExtension(iniFile);
        var iniData = Parser.ReadFile(iniFile);
        CreateOrGetIniData(iniId).Merge(iniData);
        
        if (iniData.Global.Count > 0)
        {
            _iniChanged?.Invoke(new(iniId, null, iniData.ToSection(null)));
        }
            
        foreach (var section in iniData.Sections)
        {
            if (section.Keys.Count > 0) _iniChanged?.Invoke(new(iniId, section.SectionName, iniData.ToSection(section.SectionName)));
        }
    }

    private IniData CreateOrGetIniData(string iniId)
    {
        if (!_inis.TryGetValue(iniId, out var iniData))
        {
            iniData = new();
            _inis[iniId] = iniData;
        }

        return iniData;
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
    
    private record SettingChangedArgs(string IniId, string Name, string? Section, string Value);

    private record IniChangedArgs(string IniId, string? Section, IReadOnlyDictionary<string, string> Settings);
}
