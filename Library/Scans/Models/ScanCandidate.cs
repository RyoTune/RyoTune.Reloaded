namespace RyoTune.Reloaded.Scans.Models;

internal class ScanCandidate
{
    public string? Name { get; init; }
    
    public ScanCandidateFilter? If { get; init; }

    public string? Pattern { get; init; }

    public string? Transform { get; init; }
}