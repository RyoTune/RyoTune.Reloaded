namespace RyoTune.Reloaded.Scans.Models;

internal readonly record struct ScanReceiver(Action<nint> OnSuccess, Action? OnFailure);