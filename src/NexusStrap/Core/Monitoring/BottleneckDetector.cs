using NexusStrap.Models;

namespace NexusStrap.Core.Monitoring;

public sealed class BottleneckDetector
{
    public BottleneckResult Analyze(IReadOnlyCollection<PerformanceSnapshot> snapshots)
    {
        if (snapshots.Count == 0) return new BottleneckResult { Bottleneck = BottleneckType.None };

        var avgCpu = snapshots.Average(s => s.CpuPercent);
        var avgGpu = snapshots.Average(s => s.GpuPercent);
        var avgRam = snapshots.Average(s => s.RamUsageMb);
        var avgFps = snapshots.Average(s => s.Fps);

        if (avgCpu > 90 && avgGpu < 70)
            return new BottleneckResult
            {
                Bottleneck = BottleneckType.Cpu,
                Description = $"CPU bottleneck detected (CPU: {avgCpu:F0}%, GPU: {avgGpu:F0}%)",
                Recommendation = "Try reducing physics-heavy content, enable CPU affinity optimization, or lower draw distance."
            };

        if (avgGpu > 90 && avgCpu < 70)
            return new BottleneckResult
            {
                Bottleneck = BottleneckType.Gpu,
                Description = $"GPU bottleneck detected (GPU: {avgGpu:F0}%, CPU: {avgCpu:F0}%)",
                Recommendation = "Lower graphics quality, reduce render distance, or disable shadows via FastFlags."
            };

        if (avgRam > 3500)
            return new BottleneckResult
            {
                Bottleneck = BottleneckType.Memory,
                Description = $"High memory usage ({avgRam} MB)",
                Recommendation = "Enable memory trimming, close background applications, or enable performance mode."
            };

        return new BottleneckResult
        {
            Bottleneck = BottleneckType.None,
            Description = "No significant bottleneck detected",
            Recommendation = "System is running well."
        };
    }
}

public sealed class BottleneckResult
{
    public BottleneckType Bottleneck { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public enum BottleneckType { None, Cpu, Gpu, Memory, Network }
