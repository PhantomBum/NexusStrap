using System.Diagnostics;
using System.Runtime.InteropServices;
using NexusStrap.Services;

namespace NexusStrap.Core.Performance;

public sealed class MemoryManager
{
    private readonly LogService _log;
    private Timer? _trimTimer;

    [DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSizeEx(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize, uint flags);

    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    public MemoryManager(LogService log)
    {
        _log = log;
    }

    public void TrimRobloxMemory()
    {
        try
        {
            var processes = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (var process in processes)
            {
                try
                {
                    EmptyWorkingSet(process.Handle);
                    _log.Debug("Trimmed memory for Roblox PID {Pid}", process.Id);
                }
                catch (Exception ex)
                {
                    _log.Debug("Memory trim failed for PID {Pid}: {Msg}", process.Id, ex.Message);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Memory trimming failed");
        }
    }

    public void StartPeriodicTrim(int intervalSeconds = 60)
    {
        _trimTimer = new Timer(_ => TrimRobloxMemory(), null,
            TimeSpan.FromSeconds(intervalSeconds),
            TimeSpan.FromSeconds(intervalSeconds));
        _log.Info("Periodic memory trim started (every {Interval}s)", intervalSeconds);
    }

    public void StopPeriodicTrim()
    {
        _trimTimer?.Dispose();
        _trimTimer = null;
        _log.Info("Periodic memory trim stopped");
    }

    public static long GetRobloxMemoryUsageMb()
    {
        long totalBytes = 0;
        foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
        {
            try
            {
                totalBytes += process.WorkingSet64;
            }
            catch { }
            finally
            {
                process.Dispose();
            }
        }
        return totalBytes / (1024 * 1024);
    }

    public void TrimSelfMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        using var proc = Process.GetCurrentProcess();
        EmptyWorkingSet(proc.Handle);
    }
}
