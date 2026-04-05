using System.Diagnostics;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Services;

namespace NexusStrap.Core.Stability;

public sealed class DiagnosticsEngine
{
    private readonly LogService _log;
    private readonly SettingsService _settings;

    public DiagnosticsEngine(LogService log, SettingsService settings)
    {
        _log = log;
        _settings = settings;
    }

    public async Task<DiagnosticsReport> RunDiagnosticsAsync()
    {
        var report = new DiagnosticsReport();

        report.OsVersion = Environment.OSVersion.ToString();
        report.DotNetVersion = Environment.Version.ToString();
        report.ProcessorCount = Environment.ProcessorCount;
        report.TotalMemoryMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);

        // Check Roblox installation
        var robloxPath = RegistryManager.GetCurrentRobloxVersionPath();
        report.RobloxInstalled = robloxPath is not null;
        report.RobloxPath = robloxPath;

        if (robloxPath is not null)
        {
            var exePath = Path.Combine(robloxPath, "RobloxPlayerBeta.exe");
            report.RobloxExeExists = File.Exists(exePath);
        }

        // Check protocol handler
        report.ProtocolRegistered = ProtocolHandler.IsRegistered();

        // Check NexusStrap directories
        report.NexusStrapDir = _settings.BaseDirectory;
        report.SettingsExist = File.Exists(_settings.SettingsPath);

        report.IsHealthy = report.RobloxInstalled && report.RobloxExeExists;

        return report;
    }
}

public sealed class DiagnosticsReport
{
    public string OsVersion { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public long TotalMemoryMb { get; set; }
    public bool RobloxInstalled { get; set; }
    public string? RobloxPath { get; set; }
    public bool RobloxExeExists { get; set; }
    public bool ProtocolRegistered { get; set; }
    public string NexusStrapDir { get; set; } = string.Empty;
    public bool SettingsExist { get; set; }
    public bool IsHealthy { get; set; }
}
