using NexusStrap.Core.Bootstrapper;
using NexusStrap.Services;

namespace NexusStrap.Core.Utilities;

public sealed class InstallManager
{
    private readonly VersionChecker _versionChecker;
    private readonly PackageDownloader _packageDownloader;
    private readonly PackageExtractor _packageExtractor;
    private readonly SettingsService _settings;
    private readonly LogService _log;

    public InstallManager(VersionChecker versionChecker, PackageDownloader packageDownloader,
        PackageExtractor packageExtractor, SettingsService settings, LogService log)
    {
        _versionChecker = versionChecker;
        _packageDownloader = packageDownloader;
        _packageExtractor = packageExtractor;
        _settings = settings;
        _log = log;
    }

    public async Task<bool> VerifyInstallationAsync(CancellationToken ct = default)
    {
        var versionDir = RegistryManager.GetCurrentRobloxVersionPath();
        if (versionDir is null)
        {
            _log.Warning("No Roblox installation found");
            return false;
        }

        var exePath = Path.Combine(versionDir, "RobloxPlayerBeta.exe");
        if (!File.Exists(exePath))
        {
            _log.Warning("RobloxPlayerBeta.exe missing from {Dir}", versionDir);
            return false;
        }

        _log.Info("Roblox installation verified at {Dir}", versionDir);
        return true;
    }

    public async Task RepairInstallationAsync(IProgress<double>? progress = null, CancellationToken ct = default)
    {
        _log.Info("Starting Roblox repair");

        var versionInfo = await _versionChecker.GetLatestVersionAsync(ct: ct);
        if (versionInfo is null) throw new InvalidOperationException("Cannot reach Roblox servers");

        var versionGuid = versionInfo.ClientVersionUpload;
        var downloadsDir = RegistryManager.GetRobloxDownloadsPath();
        var versionDir = Path.Combine(RegistryManager.GetRobloxVersionsPath(), versionGuid);

        var manifest = await _packageDownloader.GetPackageManifestAsync(versionGuid, ct);
        await _packageDownloader.DownloadAllPackagesAsync(versionGuid, manifest, downloadsDir, ct: ct);
        await _packageExtractor.ExtractAllAsync(downloadsDir, versionDir, manifest, ct: ct);

        _settings.RobloxState.InstalledVersionGuid = versionGuid;
        _settings.RobloxState.InstalledVersionNumber = versionInfo.Version;
        _settings.RobloxState.InstallDirectory = versionDir;
        _settings.RobloxState.LastUpdated = DateTime.Now;
        _settings.SaveRobloxState();

        _log.Info("Roblox repair complete");
    }
}
