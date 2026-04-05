using NexusStrap.Core.Bootstrapper;
using NexusStrap.Services;

namespace NexusStrap.Core.Utilities;

public sealed class SystemCleaner
{
    private readonly LogService _log;

    public SystemCleaner(LogService log)
    {
        _log = log;
    }

    public CleanupResult CleanAll()
    {
        var result = new CleanupResult();
        result.Add(CleanRobloxLogs());
        result.Add(CleanRobloxDownloads());
        result.Add(CleanRobloxTemp());
        _log.Info("Cleanup complete: {Files} files, {Size} MB freed", result.FilesDeleted, result.BytesFreed / (1024 * 1024));
        return result;
    }

    public CleanupResult CleanRobloxLogs()
    {
        return CleanDirectory(RegistryManager.GetRobloxLogsPath(), "Roblox Logs");
    }

    public CleanupResult CleanRobloxDownloads()
    {
        return CleanDirectory(RegistryManager.GetRobloxDownloadsPath(), "Roblox Downloads");
    }

    public CleanupResult CleanRobloxTemp()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Roblox");
        return CleanDirectory(tempDir, "Roblox Temp");
    }

    public CleanupResult ScanAll()
    {
        var result = new CleanupResult();
        result.Add(ScanDirectory(RegistryManager.GetRobloxLogsPath()));
        result.Add(ScanDirectory(RegistryManager.GetRobloxDownloadsPath()));
        result.Add(ScanDirectory(Path.Combine(Path.GetTempPath(), "Roblox")));
        return result;
    }

    private CleanupResult CleanDirectory(string path, string label)
    {
        var result = new CleanupResult();
        if (!Directory.Exists(path)) return result;

        try
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var info = new FileInfo(file);
                    result.BytesFreed += info.Length;
                    info.Delete();
                    result.FilesDeleted++;
                }
                catch { }
            }
            _log.Info("Cleaned {Label}: {Files} files", label, result.FilesDeleted);
        }
        catch (Exception ex)
        {
            _log.Warning("Error cleaning {Label}: {Msg}", label, ex.Message);
        }
        return result;
    }

    private static CleanupResult ScanDirectory(string path)
    {
        var result = new CleanupResult();
        if (!Directory.Exists(path)) return result;

        try
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    result.BytesFreed += new FileInfo(file).Length;
                    result.FilesDeleted++;
                }
                catch { }
            }
        }
        catch { }
        return result;
    }
}

public sealed class CleanupResult
{
    public int FilesDeleted { get; set; }
    public long BytesFreed { get; set; }

    public void Add(CleanupResult other)
    {
        FilesDeleted += other.FilesDeleted;
        BytesFreed += other.BytesFreed;
    }
}
