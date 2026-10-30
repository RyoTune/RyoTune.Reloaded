using System.Diagnostics;
using System.Security.Cryptography;

namespace RyoTune.Reloaded.Scans.Models;

internal class ScanFilter(string appFilePath)
{
    private readonly Lazy<string> _fileMd5 = new(() =>
    {
        using var stream = File.OpenRead(appFilePath);
        return Convert.ToHexString(MD5.HashData(stream));
    });

    private readonly Lazy<FileVersionInfo> _versionInfo =
        new(() => FileVersionInfo.GetVersionInfo(appFilePath));

    private readonly Lazy<string?> _fileVersion =
        new(() => FileVersionInfo.GetVersionInfo(appFilePath).FileVersion);

    private readonly Lazy<string?> _productVersion =
        new(() => FileVersionInfo.GetVersionInfo(appFilePath).ProductVersion);

    public string FileMd5 => _fileMd5.Value;
    
    public string? FileVersion => _versionInfo.Value.FileVersion;
    
    public string? ProductVersion => _versionInfo.Value.ProductVersion;
}