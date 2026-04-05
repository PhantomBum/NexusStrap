using System.Diagnostics;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Performance;

public sealed class ProcessPriorityManager
{
    private readonly LogService _log;

    public ProcessPriorityManager(LogService log)
    {
        _log = log;
    }

    public void SetRobloxPriority(ProcessPriorityLevel level)
    {
        var priority = level switch
        {
            ProcessPriorityLevel.Low => ProcessPriorityClass.Idle,
            ProcessPriorityLevel.BelowNormal => ProcessPriorityClass.BelowNormal,
            ProcessPriorityLevel.Normal => ProcessPriorityClass.Normal,
            ProcessPriorityLevel.AboveNormal => ProcessPriorityClass.AboveNormal,
            ProcessPriorityLevel.High => ProcessPriorityClass.High,
            ProcessPriorityLevel.RealTime => ProcessPriorityClass.RealTime,
            _ => ProcessPriorityClass.Normal
        };

        try
        {
            foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                try
                {
                    process.PriorityClass = priority;
                    _log.Info("Set Roblox priority to {Level} for PID {Pid}", level, process.Id);
                }
                catch (Exception ex)
                {
                    _log.Debug("Failed to set priority for PID {Pid}: {Msg}", process.Id, ex.Message);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Priority setting failed");
        }
    }

    public static ProcessPriorityClass? GetCurrentRobloxPriority()
    {
        var processes = Process.GetProcessesByName("RobloxPlayerBeta");
        if (processes.Length == 0) return null;
        try
        {
            return processes[0].PriorityClass;
        }
        finally
        {
            foreach (var p in processes) p.Dispose();
        }
    }
}
