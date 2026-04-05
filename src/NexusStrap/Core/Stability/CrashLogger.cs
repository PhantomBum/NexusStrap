using NexusStrap.Services;

namespace NexusStrap.Core.Stability;

public sealed class CrashLogger
{
    private readonly LogService _log;
    private readonly SettingsService _settings;

    public CrashLogger(LogService log, SettingsService settings)
    {
        _log = log;
        _settings = settings;
    }

    public void RegisterGlobalHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            _log.Fatal(ex!, "UNHANDLED EXCEPTION (IsTerminating: {Term})", args.IsTerminating);
            SaveCrashReport(ex);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            _log.Error(args.Exception, "UNOBSERVED TASK EXCEPTION");
            args.SetObserved();
        };
    }

    public void SaveCrashReport(Exception? ex)
    {
        if (ex is null) return;

        try
        {
            var crashDir = Path.Combine(_settings.LogsDirectory, "Crashes");
            Directory.CreateDirectory(crashDir);

            var fileName = $"crash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var path = Path.Combine(crashDir, fileName);

            var report = $"""
                NexusStrap Crash Report
                =======================
                Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
                OS: {Environment.OSVersion}
                .NET: {Environment.Version}
                Memory: {Environment.WorkingSet / (1024 * 1024)} MB

                Exception Type: {ex.GetType().FullName}
                Message: {ex.Message}
                Source: {ex.Source}

                Stack Trace:
                {ex.StackTrace}

                Inner Exception:
                {ex.InnerException}
                """;

            File.WriteAllText(path, report);
        }
        catch
        {
            // Last resort - nothing we can do
        }
    }

    public IReadOnlyList<CrashReport> GetRecentCrashes(int count = 10)
    {
        var crashDir = Path.Combine(_settings.LogsDirectory, "Crashes");
        if (!Directory.Exists(crashDir)) return Array.Empty<CrashReport>();

        return Directory.GetFiles(crashDir, "crash_*.txt")
            .OrderByDescending(f => File.GetCreationTime(f))
            .Take(count)
            .Select(f => new CrashReport
            {
                FileName = Path.GetFileName(f),
                FilePath = f,
                Timestamp = File.GetCreationTime(f),
                Preview = File.ReadAllText(f)[..Math.Min(500, File.ReadAllText(f).Length)]
            })
            .ToList();
    }
}

public sealed class CrashReport
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Preview { get; set; } = string.Empty;
}
