namespace NexusStrap.Models;

public sealed class PerformanceSnapshot
{
    public DateTime Timestamp { get; set; }
    public double Fps { get; set; }
    public double PingMs { get; set; }
    public double CpuPercent { get; set; }
    public double GpuPercent { get; set; }
    public long RamUsageMb { get; set; }
    public long VramUsageMb { get; set; }
    public double CpuTemperature { get; set; }
    public double GpuTemperature { get; set; }
    public double FrameTimeMs { get; set; }
}
