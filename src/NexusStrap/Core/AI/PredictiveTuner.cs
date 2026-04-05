using NexusStrap.Core.FastFlags;
using NexusStrap.Core.Monitoring;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.AI;

public sealed class PredictiveTuner
{
    private readonly ResourceMonitor _monitor;
    private readonly FastFlagManager _flagManager;
    private readonly NotificationService _notifications;
    private readonly LogService _log;
    private readonly SettingsService _settings;
    private bool _isActive;
    private int _consecutiveLowFps;
    private const int LowFpsThreshold = 5; // consecutive low samples before suggesting

    public bool IsActive => _isActive;

    public PredictiveTuner(ResourceMonitor monitor, FastFlagManager flagManager,
        NotificationService notifications, SettingsService settings, LogService log)
    {
        _monitor = monitor;
        _flagManager = flagManager;
        _notifications = notifications;
        _settings = settings;
        _log = log;
    }

    public void Start()
    {
        _isActive = true;
        _monitor.SnapshotCaptured += Evaluate;
        _log.Info("Predictive tuner started");
    }

    public void Stop()
    {
        _isActive = false;
        _monitor.SnapshotCaptured -= Evaluate;
        _log.Info("Predictive tuner stopped");
    }

    private void Evaluate(PerformanceSnapshot snapshot)
    {
        if (!_isActive) return;

        var threshold = _settings.Settings.FpsDropThreshold;

        if (snapshot.Fps > 0 && snapshot.Fps < threshold)
        {
            _consecutiveLowFps++;
            if (_consecutiveLowFps >= LowFpsThreshold)
            {
                _notifications.Show("Performance Alert",
                    $"FPS has been below {threshold} for extended period. Consider switching to Performance mode.",
                    NotificationLevel.Warning);
                _consecutiveLowFps = 0;
            }
        }
        else
        {
            _consecutiveLowFps = 0;
        }
    }
}
