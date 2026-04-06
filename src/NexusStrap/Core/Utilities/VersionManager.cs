using NexusStrap.Core.Bootstrapper;
using NexusStrap.Services;

namespace NexusStrap.Core.Utilities;

public sealed class VersionManager
{
    private readonly SettingsService _settings;
    private readonly LogService _log;

    public VersionManager(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
    }

    public IReadOnlyList<InstalledVersion> GetInstalledVersions()
    {
        var versionsDir = RegistryManager.GetRobloxVersionsPath();
        if (!Directory.Exists(versionsDir)) return Array.Empty<InstalledVersion>();

        return Directory.GetDirectories(versionsDir)
            .Where(d => File.Exists(Path.Combine(d, RegistryManager.RobloxPlayerExeName)))
            .Select(d => new InstalledVersion
            {
                VersionGuid = Path.GetFileName(d),
                Path = d,
                InstallDate = Directory.GetCreationTime(d),
                SizeMb = GetDirectorySizeMb(d),
                IsCurrent = Path.GetFileName(d) == _settings.RobloxState.InstalledVersionGuid
            })
            .OrderByDescending(v => v.InstallDate)
            .ToList();
    }

    public bool SwitchVersion(string versionGuid)
    {
        var versionDir = Path.Combine(RegistryManager.GetRobloxVersionsPath(), versionGuid);
        if (!Directory.Exists(versionDir) || !File.Exists(Path.Combine(versionDir, RegistryManager.RobloxPlayerExeName)))
        {
            _log.Warning("Cannot switch to version {Guid}: not found", versionGuid);
            return false;
        }

        _settings.RobloxState.InstalledVersionGuid = versionGuid;
        _settings.RobloxState.InstallDirectory = versionDir;
        _settings.SaveRobloxState();
        _log.Info("Switched to version {Guid}", versionGuid);
        return true;
    }

    public bool DeleteVersion(string versionGuid)
    {
        if (versionGuid == _settings.RobloxState.InstalledVersionGuid)
        {
            _log.Warning("Cannot delete current active version");
            return false;
        }

        var dir = Path.Combine(RegistryManager.GetRobloxVersionsPath(), versionGuid);
        if (!Directory.Exists(dir)) return false;

        try
        {
            Directory.Delete(dir, true);
            _settings.RobloxState.InstalledVersions.Remove(versionGuid);
            _settings.SaveRobloxState();
            _log.Info("Deleted version {Guid}", versionGuid);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to delete version {Guid}", versionGuid);
            return false;
        }
    }

    private static long GetDirectorySizeMb(string path)
    {
        try
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length) / (1024 * 1024);
        }
        catch { return 0; }
    }
}

public sealed class InstalledVersion
{
    public string VersionGuid { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime InstallDate { get; set; }
    public long SizeMb { get; set; }
    public bool IsCurrent { get; set; }
}
