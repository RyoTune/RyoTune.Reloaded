namespace RyoTune.Reloaded.Scans.Models;

public class ScanCandidateFilter
{
    public HashSet<string>? FileMd5 { get; init; } = [];

    public HashSet<string>? FileVersion { get; init; } = [];

    public HashSet<string>? ProductVersion { get; init; } = [];
}