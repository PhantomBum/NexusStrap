using NexusStrap.Core.Monitoring;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Stability;

public sealed class DebugOverlay
{
    private readonly ResourceMonitor _monitor;
    private readonly LogService _log;
    private bool _isVisible;

    public bool IsVisible => _isVisible;
    public PerformanceSnapshot? CurrentSnapshot => _monitor.Latest;

    public event Action<PerformanceSnapshot>? OverlayUpdated;

    public DebugOverlay(ResourceMonitor monitor, LogService log)
    {
        _monitor = monitor;
        _log = log;
    }

    public void Show()
    {
        _isVisible = true;
        _monitor.SnapshotCaptured += OnSnapshot;
        _log.Info("Debug overlay shown");
    }

    public void Hide()
    {
        _isVisible = false;
        _monitor.SnapshotCaptured -= OnSnapshot;
        _log.Info("Debug overlay hidden");
    }

    public void Toggle()
    {
        if (_isVisible) Hide(); else Show();
    }

    private void OnSnapshot(PerformanceSnapshot snapshot)
    {
        if (_isVisible)
            OverlayUpdated?.Invoke(snapshot);
    }
}
