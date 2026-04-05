using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Utilities;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class UtilitiesViewModel : ObservableObject
{
    private readonly SystemCleaner _cleaner;
    private readonly InstallManager _installManager;
    private readonly VersionManager _versionManager;

    [ObservableProperty] private string _cleanupStatus = "";
    [ObservableProperty] private long _cleanableBytes;
    [ObservableProperty] private ObservableCollection<InstalledVersion> _versions = new();
    [ObservableProperty] private string _installStatus = "";

    public UtilitiesViewModel(SystemCleaner cleaner, InstallManager installManager, VersionManager versionManager)
    {
        _cleaner = cleaner;
        _installManager = installManager;
        _versionManager = versionManager;
        RefreshVersions();
        ScanCleanable();
    }

    private void ScanCleanable()
    {
        var scan = _cleaner.ScanAll();
        CleanableBytes = scan.BytesFreed;
        CleanupStatus = $"{scan.FilesDeleted} files ({scan.BytesFreed / (1024 * 1024)} MB) can be cleaned";
    }

    private void RefreshVersions()
    {
        Versions = new ObservableCollection<InstalledVersion>(_versionManager.GetInstalledVersions());
    }

    [RelayCommand]
    private void CleanAll()
    {
        var result = _cleaner.CleanAll();
        CleanupStatus = $"Cleaned {result.FilesDeleted} files, freed {result.BytesFreed / (1024 * 1024)} MB";
        CleanableBytes = 0;
    }

    [RelayCommand]
    private void CleanLogs()
    {
        var result = _cleaner.CleanRobloxLogs();
        CleanupStatus = $"Cleaned {result.FilesDeleted} log files";
        ScanCleanable();
    }

    [RelayCommand]
    private void CleanDownloads()
    {
        var result = _cleaner.CleanRobloxDownloads();
        CleanupStatus = $"Cleaned {result.FilesDeleted} download files";
        ScanCleanable();
    }

    [RelayCommand]
    private async Task VerifyInstall()
    {
        var ok = await _installManager.VerifyInstallationAsync();
        InstallStatus = ok ? "Installation verified OK" : "Installation has issues";
    }

    [RelayCommand]
    private async Task RepairInstall()
    {
        InstallStatus = "Repairing...";
        await _installManager.RepairInstallationAsync();
        InstallStatus = "Repair complete";
        RefreshVersions();
    }

    [RelayCommand]
    private void SwitchVersion(InstalledVersion version)
    {
        _versionManager.SwitchVersion(version.VersionGuid);
        RefreshVersions();
    }

    [RelayCommand]
    private void DeleteVersion(InstalledVersion version)
    {
        _versionManager.DeleteVersion(version.VersionGuid);
        RefreshVersions();
    }
}
