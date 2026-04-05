namespace NexusStrap.PluginSDK.API;

public interface ITelemetryApi
{
    double CurrentFps { get; }
    double CurrentPing { get; }
    long MemoryUsageMb { get; }
    double CpuUsagePercent { get; }
    double GpuUsagePercent { get; }

    string? CurrentGameId { get; }
    string? CurrentServerId { get; }
    string? CurrentServerRegion { get; }
    TimeSpan SessionDuration { get; }
}
