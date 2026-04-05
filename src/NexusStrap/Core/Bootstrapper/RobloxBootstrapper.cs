using System.Diagnostics;
using NexusStrap.Core.FastFlags;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

public sealed class RobloxBootstrapper
{
    private readonly VersionChecker _versionChecker;
    private readonly PackageDownloader _packageDownloader;
    private readonly PackageExtractor _packageExtractor;
    private readonly ProtocolHandler _protocolHandler;
    private readonly FastFlagManager _fastFlagManager;
    private readonly SettingsService _settings;
    private readonly LogService _log;
    private readonly EventBus _eventBus;

    public event Action<string>? StatusChanged;
    public event Action<double>? ProgressChanged;

    public RobloxBootstrapper(
        VersionChecker versionChecker,
        PackageDownloader packageDownloader,
        PackageExtractor packageExtractor,
        ProtocolHandler protocolHandler,
        FastFlagManager fastFlagManager,
        SettingsService settings,
        LogService log,
        EventBus eventBus)
    {
        _versionChecker = versionChecker;
        _packageDownloader = packageDownloader;
        _packageExtractor = packageExtractor;
        _protocolHandler = protocolHandler;
        _fastFlagManager = fastFlagManager;
        _settings = settings;
        _log = log;
        _eventBus = eventBus;
    }

    public async Task<bool> RunAsync(string? launchUri = null, CancellationToken ct = default)
    {
        try
        {
            _log.Info("=== NexusStrap Bootstrap Started ===");

            StatusChanged?.Invoke("Checking for updates...");
            var versionInfo = await _versionChecker.GetLatestVersionAsync(
                _settings.RobloxState.Channel ?? "LIVE", ct);

            if (versionInfo is null)
            {
                _log.Error("Could not retrieve version info from Roblox servers");
                StatusChanged?.Invoke("Failed to check for updates");
                return false;
            }

            var needsUpdate = _versionChecker.IsUpdateRequired(
                _settings.RobloxState.InstalledVersionGuid,
                versionInfo.ClientVersionUpload);

            if (needsUpdate)
            {
                StatusChanged?.Invoke($"Updating Roblox to {versionInfo.Version}...");
                await UpdateRobloxAsync(versionInfo, ct);
            }
            else
            {
                _log.Info("Roblox is up to date: {Version}", versionInfo.ClientVersionUpload);
            }

            StatusChanged?.Invoke("Applying settings...");
            var versionDir = Path.Combine(
                RegistryManager.GetRobloxVersionsPath(),
                versionInfo.ClientVersionUpload);
            _fastFlagManager.ApplyFlags(versionDir);

            StatusChanged?.Invoke("Launching Roblox...");
            await LaunchRobloxAsync(versionInfo.ClientVersionUpload, launchUri, ct);

            _settings.State.LastLaunch = DateTime.Now;
            _settings.SaveState();

            _eventBus.Publish(PluginSDK.Events.LauncherEvents.OnLaunch, launchUri);
            _log.Info("=== NexusStrap Bootstrap Complete ===");
            return true;
        }
        catch (OperationCanceledException)
        {
            _log.Info("Bootstrap cancelled");
            return false;
        }
        catch (Exception ex)
        {
            _log.Fatal(ex, "Bootstrap failed");
            StatusChanged?.Invoke($"Error: {ex.Message}");
            return false;
        }
    }

    private async Task UpdateRobloxAsync(RobloxVersionInfo versionInfo, CancellationToken ct)
    {
        var versionGuid = versionInfo.ClientVersionUpload;
        var downloadsDir = RegistryManager.GetRobloxDownloadsPath();
        var versionDir = Path.Combine(RegistryManager.GetRobloxVersionsPath(), versionGuid);

        StatusChanged?.Invoke("Downloading package manifest...");
        var manifest = await _packageDownloader.GetPackageManifestAsync(versionGuid, ct);
        _log.Info("Found {Count} packages to download ({Size} MB)",
            manifest.Packages.Count, manifest.TotalCompressedSize / (1024 * 1024));

        StatusChanged?.Invoke("Downloading packages...");
        var dlProgress = new Progress<(string Package, double Progress)>(p =>
        {
            ProgressChanged?.Invoke(p.Progress * 0.5);
            StatusChanged?.Invoke($"Downloading: {p.Package}");
        });
        await _packageDownloader.DownloadAllPackagesAsync(versionGuid, manifest, downloadsDir, dlProgress, ct);

        StatusChanged?.Invoke("Extracting packages...");
        var exProgress = new Progress<(string Package, double Progress)>(p =>
        {
            ProgressChanged?.Invoke(0.5 + p.Progress * 0.5);
            StatusChanged?.Invoke($"Extracting: {p.Package}");
        });
        await _packageExtractor.ExtractAllAsync(downloadsDir, versionDir, manifest, exProgress, ct);

        _settings.RobloxState.InstalledVersionGuid = versionGuid;
        _settings.RobloxState.InstalledVersionNumber = versionInfo.Version;
        _settings.RobloxState.InstallDirectory = versionDir;
        _settings.RobloxState.LastUpdated = DateTime.Now;
        if (!_settings.RobloxState.InstalledVersions.Contains(versionGuid))
            _settings.RobloxState.InstalledVersions.Add(versionGuid);
        _settings.SaveRobloxState();

        _log.Info("Roblox updated to {Version} ({Guid})", versionInfo.Version, versionGuid);
        ProgressChanged?.Invoke(1.0);
    }

    private async Task LaunchRobloxAsync(string versionGuid, string? launchUri, CancellationToken ct)
    {
        var versionDir = Path.Combine(RegistryManager.GetRobloxVersionsPath(), versionGuid);
        var exePath = Path.Combine(versionDir, "RobloxPlayerBeta.exe");

        if (!File.Exists(exePath))
            throw new FileNotFoundException("RobloxPlayerBeta.exe not found", exePath);

        var arguments = launchUri ?? "roblox-player:1+launchmode:app";

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            UseShellExecute = false,
            WorkingDirectory = versionDir
        };

        _log.Info("Launching {Exe} with args: {Args}", exePath, arguments);
        var process = Process.Start(psi);
        if (process is not null)
        {
            await Task.Delay(500, ct);
            _log.Info("Roblox launched with PID {Pid}", process.Id);
        }
    }
}
