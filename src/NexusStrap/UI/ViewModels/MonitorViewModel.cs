using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using NexusStrap.Core.Monitoring;
using NexusStrap.Core.Stability;
using NexusStrap.Models;
using SkiaSharp;

namespace NexusStrap.UI.ViewModels;

public partial class MonitorViewModel : ObservableObject
{
    private readonly ResourceMonitor _monitor;
    private readonly BottleneckDetector _bottleneckDetector;
    private readonly DebugOverlay _debugOverlay;

    private readonly ObservableCollection<ObservableValue> _cpuHistory = new();
    private readonly ObservableCollection<ObservableValue> _gpuHistory = new();
    private readonly ObservableCollection<ObservableValue> _ramHistory = new();

    [ObservableProperty] private double _cpuPercent;
    [ObservableProperty] private double _gpuPercent;
    [ObservableProperty] private long _ramMb;
    [ObservableProperty] private double _fps;
    [ObservableProperty] private double _pingMs;
    [ObservableProperty] private string _bottleneckInfo = "Analyzing...";
    [ObservableProperty] private bool _overlayEnabled;

    public ISeries[] CpuSeries { get; }
    public ISeries[] GpuSeries { get; }
    public ISeries[] RamSeries { get; }

    public MonitorViewModel(ResourceMonitor monitor, BottleneckDetector bottleneckDetector, DebugOverlay debugOverlay)
    {
        _monitor = monitor;
        _bottleneckDetector = bottleneckDetector;
        _debugOverlay = debugOverlay;

        CpuSeries = new ISeries[] { new LineSeries<ObservableValue>
        {
            Values = _cpuHistory,
            Stroke = new SolidColorPaint(SKColor.Parse("#06B6D4"), 2),
            Fill = new SolidColorPaint(SKColor.Parse("#06B6D4").WithAlpha(30)),
            GeometrySize = 0, LineSmoothness = 0.5
        }};

        GpuSeries = new ISeries[] { new LineSeries<ObservableValue>
        {
            Values = _gpuHistory,
            Stroke = new SolidColorPaint(SKColor.Parse("#10B981"), 2),
            Fill = new SolidColorPaint(SKColor.Parse("#10B981").WithAlpha(30)),
            GeometrySize = 0, LineSmoothness = 0.5
        }};

        RamSeries = new ISeries[] { new LineSeries<ObservableValue>
        {
            Values = _ramHistory,
            Stroke = new SolidColorPaint(SKColor.Parse("#F59E0B"), 2),
            Fill = new SolidColorPaint(SKColor.Parse("#F59E0B").WithAlpha(30)),
            GeometrySize = 0, LineSmoothness = 0.5
        }};

        _monitor.SnapshotCaptured += OnSnapshot;
    }

    private void OnSnapshot(PerformanceSnapshot s)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            CpuPercent = s.CpuPercent; GpuPercent = s.GpuPercent;
            RamMb = s.RamUsageMb; Fps = s.Fps; PingMs = s.PingMs;

            _cpuHistory.Add(new ObservableValue(s.CpuPercent));
            _gpuHistory.Add(new ObservableValue(s.GpuPercent));
            _ramHistory.Add(new ObservableValue(s.RamUsageMb));

            while (_cpuHistory.Count > 120) _cpuHistory.RemoveAt(0);
            while (_gpuHistory.Count > 120) _gpuHistory.RemoveAt(0);
            while (_ramHistory.Count > 120) _ramHistory.RemoveAt(0);

            var bottleneck = _bottleneckDetector.Analyze(_monitor.History);
            BottleneckInfo = $"{bottleneck.Description}\n{bottleneck.Recommendation}";
        });
    }

    partial void OnOverlayEnabledChanged(bool value)
    {
        if (value) _debugOverlay.Show(); else _debugOverlay.Hide();
    }
}
