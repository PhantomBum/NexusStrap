using System.Collections.Concurrent;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Monitoring;

public sealed class ResourceMonitor : IDisposable
{
    private readonly SystemMetrics _metrics;
    private readonly AlertEngine _alertEngine;
    private readonly LogService _log;
    private Timer? _timer;
    private bool _isRunning;

    private readonly ConcurrentQueue<PerformanceSnapshot> _history = new();
    private const int MaxHistory = 120; // 60 seconds at 500ms intervals

    public PerformanceSnapshot? Latest { get; private set; }
    public IReadOnlyCollection<PerformanceSnapshot> History => _history.ToArray();
    public bool IsRunning => _isRunning;

    public event Action<PerformanceSnapshot>? SnapshotCaptured;

    public ResourceMonitor(SystemMetrics metrics, AlertEngine alertEngine, LogService log)
    {
        _metrics = metrics;
        _alertEngine = alertEngine;
        _log = log;
    }

    public void Start(int intervalMs = 500)
    {
        if (_isRunning) return;
        _isRunning = true;
        _timer = new Timer(Tick, null, 0, intervalMs);
        _log.Info("Resource monitor started (interval: {Ms}ms)", intervalMs);
    }

    public void Stop()
    {
        _isRunning = false;
        _timer?.Dispose();
        _timer = null;
        _log.Info("Resource monitor stopped");
    }

    private void Tick(object? state)
    {
        if (!_isRunning) return;

        try
        {
            var snapshot = _metrics.Capture();
            Latest = snapshot;

            _history.Enqueue(snapshot);
            while (_history.Count > MaxHistory)
                _history.TryDequeue(out _);

            _alertEngine.Evaluate(snapshot);
            SnapshotCaptured?.Invoke(snapshot);
        }
        catch (Exception ex)
        {
            _log.Debug("Resource monitor tick failed: {Msg}", ex.Message);
        }
    }

    public PerformanceSnapshot GetAverage()
    {
        var snapshots = _history.ToArray();
        if (snapshots.Length == 0) return new PerformanceSnapshot();

        return new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            CpuPercent = snapshots.Average(s => s.CpuPercent),
            GpuPercent = snapshots.Average(s => s.GpuPercent),
            RamUsageMb = (long)snapshots.Average(s => s.RamUsageMb),
            Fps = snapshots.Average(s => s.Fps),
            PingMs = snapshots.Average(s => s.PingMs),
            FrameTimeMs = snapshots.Average(s => s.FrameTimeMs)
        };
    }

    public void Dispose()
    {
        Stop();
        _metrics.Dispose();
    }
}
