using System.Diagnostics;
using System.Runtime.InteropServices;
using NexusStrap.Services;

namespace NexusStrap.Core.Launch;

public sealed class MultiInstanceManager : IDisposable
{
    private readonly LogService _log;
    private Mutex? _singletonMutex;
    private bool _isHolding;

    private const string RobloxMutexName = "ROBLOX_singletonMutex";

    public MultiInstanceManager(LogService log)
    {
        _log = log;
    }

    public bool IsEnabled { get; private set; }

    public bool AcquireMutex()
    {
        try
        {
            _singletonMutex = new Mutex(true, RobloxMutexName, out bool createdNew);
            if (createdNew)
            {
                _isHolding = true;
                IsEnabled = true;
                _log.Info("Acquired Roblox singleton mutex for multi-instance support");
                return true;
            }
            _log.Warning("Roblox singleton mutex already held by another process");
            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to acquire Roblox singleton mutex");
            return false;
        }
    }

    public void ReleaseMutex()
    {
        if (_isHolding && _singletonMutex is not null)
        {
            try
            {
                _singletonMutex.ReleaseMutex();
                _isHolding = false;
                IsEnabled = false;
                _log.Info("Released Roblox singleton mutex");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to release Roblox singleton mutex");
            }
        }
    }

    public static int GetRunningRobloxInstanceCount()
    {
        return Process.GetProcessesByName("RobloxPlayerBeta").Length;
    }

    public static IReadOnlyList<Process> GetRunningRobloxInstances()
    {
        return Process.GetProcessesByName("RobloxPlayerBeta");
    }

    public async Task WaitForAllInstancesClosedAsync(CancellationToken ct = default)
    {
        while (GetRunningRobloxInstanceCount() > 0 && !ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct);
        }
    }

    public void Dispose()
    {
        ReleaseMutex();
        _singletonMutex?.Dispose();
    }
}
