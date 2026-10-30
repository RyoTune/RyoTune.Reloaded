namespace RyoTune.Reloaded.Scans.Models;

internal class Scan(ScanFilter filter)
{
    private readonly List<ScanCandidate> _candidates = [];
    private readonly List<ScanReceiver> _receivers = [];
    private readonly List<ScanCandidateResult> _results = [];
    private nint _default;

    public bool IsRequired { get; set; } = true;

    public IReadOnlyList<ScanCandidate> Candidates => _candidates;

    public IReadOnlyList<ScanReceiver> Receivers => _receivers;

    public IReadOnlyList<ScanCandidateResult> Results
    {
        get
        {
            lock (_results) return _results;
        }
    }

    public void AddCandidate(ScanCandidate candidate)
    {
        if (!IsValid(candidate)) return;
        _candidates.Add(candidate);
    }
    
    public void AddReceiver(Action<nint> onSuccess, Action? onFailure)
        => _receivers.Add(new(onSuccess, onFailure));

    public void SetCandidateResult(ScanCandidateResult result)
    {
        lock (_results) _results.Add(result);
    }
    
    public void SetDefault(nint value) => _default = value;
    
    public bool TryGetDefault(out nint value)
    {
        value = _default;
        return _default != 0;
    }

    private bool IsValid(ScanCandidate candidate)
    {
        var conditions = candidate.If;
        if (conditions == null) return true;

        return (conditions.FileMd5 == null ||
                conditions.FileMd5.Contains(filter.FileMd5))
               && (conditions.FileVersion == null ||
                   filter.FileVersion is { } fileVersion &&
                   conditions.FileVersion.Contains(fileVersion))
               && (conditions.ProductVersion == null ||
                   filter.ProductVersion is { } productVersion &&
                   conditions.ProductVersion.Contains(productVersion));
    }
}