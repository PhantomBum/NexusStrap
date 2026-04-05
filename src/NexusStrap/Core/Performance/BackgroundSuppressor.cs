using System.Diagnostics;
using NexusStrap.Services;

namespace NexusStrap.Core.Performance;

public sealed class BackgroundSuppressor
{
    private readonly LogService _log;
    private Timer? _timer;
    private bool _isActive;

    private static readonly string[] SuppressibleProcesses =
    {
        "SearchUI", "SearchApp", "ShellExperienceHost", "StartMenuExperienceHost",
        "TextInputHost", "YourPhone", "GameBar", "GameBarPresenceWriter",
        "backgroundTaskHost", "CompatTelRunner"
    };

    public bool IsActive => _isActive;

    public BackgroundSuppressor(LogService log)
    {
        _log = log;
    }

    public void Start()
    {
        if (_isActive) return;
        _isActive = true;
        _timer = new Timer(SuppressProcesses, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        _log.Info("Background suppressor started");
    }

    public void Stop()
    {
        _isActive = false;
        _timer?.Dispose();
        _timer = null;
        _log.Info("Background suppressor stopped");
    }

    private void SuppressProcesses(object? state)
    {
        if (!_isActive) return;

        foreach (var name in SuppressibleProcesses)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName(name))
                {
                    try
                    {
                        if (proc.PriorityClass != ProcessPriorityClass.Idle)
                        {
                            proc.PriorityClass = ProcessPriorityClass.Idle;
                        }
                    }
                    catch { }
                    finally
                    {
                        proc.Dispose();
                    }
                }
            }
            catch { }
        }
    }
}
