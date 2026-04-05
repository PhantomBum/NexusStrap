using System.Diagnostics;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Monitoring;

public sealed class SystemMetrics : IDisposable
{
    private readonly LogService _log;
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _ramCounter;
    private Process? _robloxProcess;

    public SystemMetrics(LogService log)
    {
        _log = log;
        InitCounters();
    }

    private void InitCounters()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            _cpuCounter.NextValue(); // first call always returns 0
        }
        catch (Exception ex)
        {
            _log.Warning("Failed to initialize performance counters: {Msg}", ex.Message);
        }
    }

    public PerformanceSnapshot Capture()
    {
        RefreshRobloxProcess();

        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            CpuPercent = GetCpuUsage(),
            RamUsageMb = GetRobloxMemoryMb(),
        };

        try
        {
            if (_ramCounter is not null)
            {
                var availableMb = _ramCounter.NextValue();
                // snapshot already has RamUsageMb from Roblox process
            }
        }
        catch { }

        return snapshot;
    }

    private double GetCpuUsage()
    {
        try
        {
            return _cpuCounter?.NextValue() ?? 0;
        }
        catch { return 0; }
    }

    private long GetRobloxMemoryMb()
    {
        try
        {
            if (_robloxProcess is not null && !_robloxProcess.HasExited)
            {
                _robloxProcess.Refresh();
                return _robloxProcess.WorkingSet64 / (1024 * 1024);
            }
        }
        catch { }
        return 0;
    }

    private void RefreshRobloxProcess()
    {
        if (_robloxProcess is not null && !_robloxProcess.HasExited) return;

        _robloxProcess?.Dispose();
        _robloxProcess = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _ramCounter?.Dispose();
        _robloxProcess?.Dispose();
    }
}
