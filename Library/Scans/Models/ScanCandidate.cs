namespace RyoTune.Reloaded.Scans.Models;

public class ScanCandidate
{
    public string? Name { get; init; }
    
    public ScanCandidateFilter? If { get; init; }

    public string? Pattern { get; init; }

    public string? Transform { get; init; }
}