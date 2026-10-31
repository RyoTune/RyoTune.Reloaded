using RyoTune.Reloaded.Inis;
using RyoTune.Reloaded.Scans;
using RyoTune.Reloaded.Scans.Models;
using RyoTune.Reloaded.Serialization.Yaml.Interfaces;

namespace RyoTune.Reloaded.Common.Configs;

internal class ScanConfigHandler(IScans scans) : IConfigHandler
{
    private const string ResultSettingTag = "_RESULT";
    private const string DisabledScanValue = "DISABLED";
    
    public IYamlDeserializer? YamlDeserializer { get; init; }
    
    public bool CanHandle(string configId) => configId == "scans";

    public void Handle(string configId, string newConfigFile)
    {
        var ext = Path.GetExtension(newConfigFile).ToLowerInvariant();
        if (ext == ".ini")
        {
            var iniData = SettingsConfigHandler.IniParser.ReadFile(newConfigFile);
            var scansSection = iniData.ToSection("Scans");
            
            foreach (var (scanId, value) in scansSection)
            {
                if (scanId.EndsWith(ResultSettingTag)) continue;
                
                if (value == DisabledScanValue)
                {
                    scans.RemoveScan(scanId);
                    continue;
                }
                
                scansSection.TryGetValue($"{scanId}{ResultSettingTag}", out var transform);
                
                scans.ResetScanCandidates(scanId);
                scans.AddScanCandidate(scanId, new ScanCandidate { Pattern = value, Transform = transform });
            }
        }
        else if (YamlDeserializer != null && ext == ".yaml")
        {
            var yamlScans = YamlDeserializer.Deserialize<Dictionary<string, ScanEntry>>(File.ReadAllText(newConfigFile));
            foreach (var (scanId, scanEntry) in yamlScans)
            {
                if (scanEntry.Disabled)
                {
                    scans.RemoveScan(scanId);
                    continue;
                }
                
                if (scanEntry.Required != null)
                    scans.SetScanRequired(scanId, scanEntry.Required.Value);
                
                scans.ResetScanCandidates(scanId);
                foreach (var candidate in scanEntry.Candidates)
                    if (candidate.Pattern != null) scans.AddScanCandidate(scanId, candidate);
            }
        }
    }
    
    private class ScanEntry
    {
        public bool? Required { get; init; }

        public bool Disabled { get; init; }

        public List<ScanCandidate> Candidates { get; init; } = [];
    }
}