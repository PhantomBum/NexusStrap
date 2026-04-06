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
                var offlineDir = RegistryManager.FindRobloxPlayerDirectory(
                    preferredVersionGuid: _settings.RobloxState.InstalledVersionGuid,
                    installDirectoryHint: _settings.RobloxState.InstallDirectory);

                if (offlineDir is null)
                {
                    _log.Error("Could not reach Roblox version API and no local RobloxPlayer install was found under {Versions}",
                        RegistryManager.GetRobloxVersionsPath());
                    StatusChanged?.Invoke("Roblox not installed (or install Roblox from roblox.com)");
                    return false;
                }

                _log.Warning("Version API unavailable: launching from local install {Dir}", offlineDir);
                StatusChanged?.Invoke("Applying settings...");
                _fastFlagManager.ApplyFlags(offlineDir);
                StatusChanged?.Invoke("Launching Roblox...");
                await LaunchRobloxFromDirectoryAsync(offlineDir, launchUri, ct);
                SyncRobloxStateFromInstallDirectory(offlineDir);

                _settings.State.LastLaunch = DateTime.Now;
                _settings.SaveState();
                _eventBus.Publish(PluginSDK.Events.LauncherEvents.OnLaunch, launchUri);
                _log.Info("=== NexusStrap Bootstrap Complete (offline) ===");
                return true;
            }

            var needsUpdate = _versionChecker.IsUpdateRequired(
                _settings.RobloxState.InstalledVersionGuid,
                versionInfo.ClientVersionUpload);

            var localLatestMatch = RegistryManager.FindRobloxPlayerDirectory(
                preferredVersionGuid: versionInfo.ClientVersionUpload,
                installDirectoryHint: null);

            if (needsUpdate && localLatestMatch is null)
            {
                StatusChanged?.Invoke($"Updating Roblox to {versionInfo.Version}...");
                await UpdateRobloxAsync(versionInfo, ct);
            }
            else if (needsUpdate && localLatestMatch is not null)
            {
                _log.Info(
                    "Latest Roblox build already on disk at {Dir}; skipping download (sync NexusStrap state)",
                    localLatestMatch);
                _settings.RobloxState.InstalledVersionGuid = versionInfo.ClientVersionUpload;
                _settings.RobloxState.InstallDirectory = localLatestMatch;
                _settings.RobloxState.InstalledVersionNumber = versionInfo.Version;
                if (!_settings.RobloxState.InstalledVersions.Contains(versionInfo.ClientVersionUpload))
                    _settings.RobloxState.InstalledVersions.Add(versionInfo.ClientVersionUpload);
                _settings.SaveRobloxState();
            }
            else
            {
                _log.Info("Roblox is up to date: {Version}", versionInfo.ClientVersionUpload);
            }

            var versionDir = RegistryManager.FindRobloxPlayerDirectory(
                preferredVersionGuid: versionInfo.ClientVersionUpload,
                installDirectoryHint: _settings.RobloxState.InstallDirectory);

            if (versionDir is null)
            {
                versionDir = RegistryManager.GetCurrentRobloxVersionPath();
                if (versionDir is not null)
                {
                    _log.Warning(
                        "Expected player folder for {Expected} not found; launching from latest local install {Actual}",
                        versionInfo.ClientVersionUpload,
                        versionDir);
                }
            }

            if (versionDir is null)
            {
                _log.Error("RobloxPlayer not found under {Versions}", RegistryManager.GetRobloxVersionsPath());
                StatusChanged?.Invoke("Roblox player not found — install Roblox from roblox.com");
                return false;
            }

            StatusChanged?.Invoke("Applying settings...");
            _fastFlagManager.ApplyFlags(versionDir);

            StatusChanged?.Invoke("Launching Roblox...");
            await LaunchRobloxFromDirectoryAsync(versionDir, launchUri, ct);
            SyncRobloxStateFromInstallDirectory(versionDir);

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

    private async Task LaunchRobloxFromDirectoryAsync(string versionDir, string? launchUri, CancellationToken ct)
    {
        var exePath = Path.Combine(versionDir, RegistryManager.RobloxPlayerExeName);

        if (!File.Exists(exePath))
            throw new FileNotFoundException($"{RegistryManager.RobloxPlayerExeName} not found", exePath);

        var arguments = launchUri ?? "roblox-player:1+launchmode:app";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = versionDir
            };

            _log.Info("Launching {Exe} with args: {Args}", exePath, arguments);
            var process = Process.Start(psi);
            if (process is null)
            {
                _log.Warning("Process.Start returned null for {Exe}; opening roblox-player via shell.", exePath);
                StartRobloxWithProtocol(arguments);
            }
            else
            {
                await Task.Delay(500, ct);
                _log.Info("Roblox launched with PID {Pid}", process.Id);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex, "Direct player launch failed; trying registered roblox-player protocol.");
            StartRobloxWithProtocol(arguments);
            await Task.Delay(400, ct);
        }
    }

    /// <summary>Uses the OS-registered <c>roblox-player:</c> / protocol handler (same as Start menu shortcut).</summary>
    private static void StartRobloxWithProtocol(string launchArguments)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = launchArguments,
            UseShellExecute = true
        });
    }

    /// <summary>Keep saved settings aligned with the folder we actually launched from.</summary>
    private void SyncRobloxStateFromInstallDirectory(string versionDir)
    {
        var guid = Path.GetFileName(versionDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrEmpty(guid)) return;

        _settings.RobloxState.InstalledVersionGuid = guid;
        _settings.RobloxState.InstallDirectory = versionDir;
        if (!_settings.RobloxState.InstalledVersions.Contains(guid))
            _settings.RobloxState.InstalledVersions.Add(guid);
        _settings.SaveRobloxState();
    }
}
