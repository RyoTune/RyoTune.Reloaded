using IniParser.Model;

namespace RyoTune.Reloaded.Inis;

internal static class IniDataExtensions
{
    public static Dictionary<string, string> ToSection(this IniData ini, string? sectionName)
    {
        if (sectionName == null)
        {
            return ini.Global.ToDictionary(x => x.KeyName, x => x.Value);
        }

        if (ini.Sections.FirstOrDefault(x => x.SectionName == sectionName) is { } section)
        {
            return section.Keys.ToDictionary(x => x.KeyName, x => x.Value);
        }

        return [];
    }
}