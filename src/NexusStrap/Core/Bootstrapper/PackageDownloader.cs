using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

public sealed class PackageDownloader
{
    private readonly HttpService _http;
    private readonly LogService _log;

    public PackageDownloader(HttpService http, LogService log)
    {
        _http = http;
        _log = log;
    }

    public async Task<PackageManifest> GetPackageManifestAsync(string versionGuid, CancellationToken ct = default)
    {
        var manifestUrl = $"https://setup.rbxcdn.com/{versionGuid}-rbxPkgManifest.txt";
        _log.Info("Downloading package manifest for {Version}", versionGuid);

        var content = await _http.GetStringAsync(manifestUrl, ct);
        return ParseManifest(content);
    }

    public async Task DownloadPackageAsync(string versionGuid, PackageInfo package,
        string downloadDir, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var relativePath = $"{versionGuid}-{package.Name}";
        var destinationPath = Path.Combine(downloadDir, package.Name);

        if (File.Exists(destinationPath))
        {
            var existingInfo = new FileInfo(destinationPath);
            if (existingInfo.Length == package.CompressedSize)
            {
                _log.Debug("Package already downloaded: {Name}", package.Name);
                progress?.Report(1.0);
                return;
            }
        }

        _log.Info("Downloading package: {Name} ({Size} bytes)", package.Name, package.CompressedSize);
        var usedCdn = await _http.DownloadFromCdnAsync(relativePath, destinationPath, progress, ct);

        if (usedCdn is null)
            throw new InvalidOperationException($"Failed to download package {package.Name} from any CDN");

        _log.Debug("Downloaded {Name} from {Cdn}", package.Name, usedCdn);
    }

    public async Task DownloadAllPackagesAsync(string versionGuid, PackageManifest manifest,
        string downloadDir, IProgress<(string Package, double Progress)>? progress = null,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(downloadDir);

        for (int i = 0; i < manifest.Packages.Count; i++)
        {
            var pkg = manifest.Packages[i];
            var pkgProgress = new Progress<double>(p =>
                progress?.Report((pkg.Name, (i + p) / manifest.Packages.Count)));

            await DownloadPackageAsync(versionGuid, pkg, downloadDir, pkgProgress, ct);
        }
    }

    private static PackageManifest ParseManifest(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var packages = new List<PackageInfo>();

        // rbxPkgManifest.txt: groups of 4 lines per package
        // Line 0: package filename
        // Line 1: MD5 hash
        // Line 2: compressed size
        // Line 3: uncompressed size
        for (int i = 0; i + 3 < lines.Length; i += 4)
        {
            if (long.TryParse(lines[i + 2], out var compSize) &&
                long.TryParse(lines[i + 3], out var uncompSize))
            {
                packages.Add(new PackageInfo
                {
                    Name = lines[i],
                    Md5Hash = lines[i + 1],
                    CompressedSize = compSize,
                    UncompressedSize = uncompSize
                });
            }
        }

        return new PackageManifest { Packages = packages };
    }
}

public sealed class PackageManifest
{
    public List<PackageInfo> Packages { get; set; } = new();
    public long TotalCompressedSize => Packages.Sum(p => p.CompressedSize);
    public long TotalUncompressedSize => Packages.Sum(p => p.UncompressedSize);
}

public sealed class PackageInfo
{
    public string Name { get; set; } = string.Empty;
    public string Md5Hash { get; set; } = string.Empty;
    public long CompressedSize { get; set; }
    public long UncompressedSize { get; set; }
}
