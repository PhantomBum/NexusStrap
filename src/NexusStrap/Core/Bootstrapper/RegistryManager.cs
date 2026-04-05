using Microsoft.Win32;
using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

public sealed class RegistryManager
{
    private readonly LogService _log;

    private const string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\NexusStrap";

    public RegistryManager(LogService log)
    {
        _log = log;
    }

    public void RegisterApp(string installPath, string version)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(UninstallKeyPath);
            key.SetValue("DisplayName", "NexusStrap");
            key.SetValue("DisplayVersion", version);
            key.SetValue("InstallLocation", installPath);
            key.SetValue("Publisher", "NexusStrap");
            key.SetValue("UninstallString", $"\"{Path.Combine(installPath, "NexusStrap.exe")}\" --uninstall");
            key.SetValue("NoModify", 1, RegistryValueKind.DWord);
            key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            _log.Info("Registered NexusStrap in Add/Remove Programs");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to register app in registry");
        }
    }

    public void UnregisterApp()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(UninstallKeyPath, false);
            _log.Info("Unregistered NexusStrap from Add/Remove Programs");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to unregister app from registry");
        }
    }

    public static string? GetRobloxInstallPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var robloxPath = Path.Combine(localAppData, "Roblox");
        return Directory.Exists(robloxPath) ? robloxPath : null;
    }

    public static string GetRobloxVersionsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Roblox", "Versions");
    }

    public static string? GetCurrentRobloxVersionPath()
    {
        var versionsPath = GetRobloxVersionsPath();
        if (!Directory.Exists(versionsPath)) return null;

        return Directory.GetDirectories(versionsPath)
            .Where(d => File.Exists(Path.Combine(d, "RobloxPlayerBeta.exe")))
            .OrderByDescending(d => Directory.GetLastWriteTime(d))
            .FirstOrDefault();
    }

    public static string GetRobloxDownloadsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Roblox", "Downloads");
    }

    public static string GetRobloxLogsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Roblox", "Logs");
    }
}
