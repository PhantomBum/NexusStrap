using System.Diagnostics;
using NexusStrap.Services;

namespace NexusStrap.Core.Performance;

public sealed class CpuAffinityManager
{
    private readonly LogService _log;

    public CpuAffinityManager(LogService log)
    {
        _log = log;
    }

    public int ProcessorCount => Environment.ProcessorCount;

    public void SetRobloxAffinity(long affinityMask)
    {
        try
        {
            foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                try
                {
                    process.ProcessorAffinity = (IntPtr)affinityMask;
                    _log.Info("Set CPU affinity for PID {Pid} to 0x{Mask:X}", process.Id, affinityMask);
                }
                catch (Exception ex)
                {
                    _log.Debug("Failed to set affinity for PID {Pid}: {Msg}", process.Id, ex.Message);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "CPU affinity setting failed");
        }
    }

    public void SetRobloxAffinityByCores(int[] coreIndices)
    {
        long mask = 0;
        foreach (var core in coreIndices)
        {
            if (core >= 0 && core < 64)
                mask |= 1L << core;
        }
        SetRobloxAffinity(mask);
    }

    public void ResetRobloxAffinity()
    {
        long allCores = (1L << ProcessorCount) - 1;
        SetRobloxAffinity(allCores);
        _log.Info("Reset CPU affinity to all cores");
    }

    public static long GetCurrentAffinity()
    {
        var processes = Process.GetProcessesByName("RobloxPlayerBeta");
        if (processes.Length == 0) return 0;
        try
        {
            return (long)processes[0].ProcessorAffinity;
        }
        finally
        {
            foreach (var p in processes) p.Dispose();
        }
    }
}
