namespace RyoTune.Reloaded.Scans.Models;

internal record ScanCandidateResult(int Order, ScanCandidate Candidate, bool IsFound, nint Value);