using System.Management;
using NexusStrap.Services;

namespace NexusStrap.Core.AI;

public sealed class HardwareProfiler
{
    private readonly LogService _log;

    public HardwareProfiler(LogService log)
    {
        _log = log;
    }

    public HardwareProfile GetProfile()
    {
        var profile = new HardwareProfile
        {
            CpuName = GetWmiString("Win32_Processor", "Name"),
            CpuCores = Environment.ProcessorCount,
            TotalRamMb = GetTotalRamMb(),
            GpuName = GetWmiString("Win32_VideoController", "Name"),
            GpuVramMb = GetGpuVramMb(),
            OsVersion = Environment.OSVersion.ToString()
        };

        profile.Tier = ClassifyTier(profile);
        _log.Info("Hardware profile: {Cpu}, {Ram}MB RAM, {Gpu}, Tier={Tier}",
            profile.CpuName, profile.TotalRamMb, profile.GpuName, profile.Tier);

        return profile;
    }

    private static HardwareTier ClassifyTier(HardwareProfile profile)
    {
        if (profile.TotalRamMb < 4096 || profile.CpuCores <= 2)
            return HardwareTier.Low;
        if (profile.TotalRamMb < 8192 || profile.CpuCores <= 4)
            return HardwareTier.Medium;
        if (profile.TotalRamMb < 16384 || profile.CpuCores <= 8)
            return HardwareTier.High;
        return HardwareTier.Ultra;
    }

    private static string GetWmiString(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (var obj in searcher.Get())
            {
                return obj[property]?.ToString() ?? "Unknown";
            }
        }
        catch { }
        return "Unknown";
    }

    private static long GetTotalRamMb()
    {
        return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
    }

    private static long GetGpuVramMb()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT AdapterRAM FROM Win32_VideoController");
            foreach (var obj in searcher.Get())
            {
                if (obj["AdapterRAM"] is uint ram) return ram / (1024 * 1024);
            }
        }
        catch { }
        return 0;
    }
}

public sealed class HardwareProfile
{
    public string CpuName { get; set; } = "Unknown";
    public int CpuCores { get; set; }
    public long TotalRamMb { get; set; }
    public string GpuName { get; set; } = "Unknown";
    public long GpuVramMb { get; set; }
    public string OsVersion { get; set; } = string.Empty;
    public HardwareTier Tier { get; set; }
}

public enum HardwareTier { Low, Medium, High, Ultra }
