using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Core.Launch;
using NexusStrap.Core.Monitoring;
using NexusStrap.Core.Performance;
using NexusStrap.Models;
using NexusStrap.Services;
using SkiaSharp;

namespace NexusStrap.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ResourceMonitor _monitor;
    private readonly LaunchController _launcher;
    private readonly PerformancePresetManager _presetManager;
    private readonly BottleneckDetector _bottleneckDetector;
    private readonly SettingsService _settings;

    private readonly ObservableCollection<ObservableValue> _fpsValues = new();
    private readonly ObservableCollection<ObservableValue> _cpuValues = new();
    private readonly ObservableCollection<ObservableValue> _ramValues = new();

    [ObservableProperty] private double _currentFps;
    [ObservableProperty] private double _currentCpu;
    [ObservableProperty] private long _currentRam;
    [ObservableProperty] private double _currentPing;
    [ObservableProperty] private double _currentGpu;
    [ObservableProperty] private string _currentGame = "None";
    [ObservableProperty] private string _currentServer = "None";
    [ObservableProperty] private string _serverRegion = "Unknown";
    [ObservableProperty] private string _sessionTime = "00:00:00";
    [ObservableProperty] private string _bottleneckStatus = "Analyzing...";
    [ObservableProperty] private string _robloxStatus = "Not Running";

    public ISeries[] FpsSeries { get; }
    public ISeries[] CpuSeries { get; }
    public ISeries[] RamSeries { get; }

    public DashboardViewModel(
        ResourceMonitor monitor,
        LaunchController launcher,
        PerformancePresetManager presetManager,
        BottleneckDetector bottleneckDetector,
        SettingsService settings)
    {
        _monitor = monitor;
        _launcher = launcher;
        _presetManager = presetManager;
        _bottleneckDetector = bottleneckDetector;
        _settings = settings;

        FpsSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = _fpsValues,
                Fill = new SolidColorPaint(SKColor.Parse("#7C3AED").WithAlpha(40)),
                Stroke = new SolidColorPaint(SKColor.Parse("#7C3AED"), 2),
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        };

        CpuSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = _cpuValues,
                Fill = new SolidColorPaint(SKColor.Parse("#06B6D4").WithAlpha(40)),
                Stroke = new SolidColorPaint(SKColor.Parse("#06B6D4"), 2),
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        };

        RamSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = _ramValues,
                Fill = new SolidColorPaint(SKColor.Parse("#F59E0B").WithAlpha(40)),
                Stroke = new SolidColorPaint(SKColor.Parse("#F59E0B"), 2),
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        };

        _monitor.SnapshotCaptured += OnSnapshot;
        _monitor.Start(settings.Settings.MonitoringIntervalMs);
    }

    private void OnSnapshot(PerformanceSnapshot snapshot)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            CurrentFps = snapshot.Fps;
            CurrentCpu = snapshot.CpuPercent;
            CurrentRam = snapshot.RamUsageMb;
            CurrentPing = snapshot.PingMs;
            CurrentGpu = snapshot.GpuPercent;

            _fpsValues.Add(new ObservableValue(snapshot.Fps));
            _cpuValues.Add(new ObservableValue(snapshot.CpuPercent));
            _ramValues.Add(new ObservableValue(snapshot.RamUsageMb));

            while (_fpsValues.Count > 120) _fpsValues.RemoveAt(0);
            while (_cpuValues.Count > 120) _cpuValues.RemoveAt(0);
            while (_ramValues.Count > 120) _ramValues.RemoveAt(0);

            RobloxStatus = Core.Launch.MultiInstanceManager.GetRunningRobloxInstanceCount() > 0
                ? "Running" : "Not Running";

            var bottleneck = _bottleneckDetector.Analyze(_monitor.History);
            BottleneckStatus = bottleneck.Description;
        });
    }

    [RelayCommand]
    private async Task LaunchRoblox()
    {
        try
        {
            await _launcher.LaunchDesktopAppAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not launch Roblox.\n\n{ex.Message}",
                "NexusStrap",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    /// <summary>Opens the Roblox website (Voidstrap-style companion to the in-app Play button).</summary>
    [RelayCommand]
    private void OpenRobloxWebsite()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.roblox.com/home",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not open the browser.\n\n{ex.Message}",
                "NexusStrap",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void ApplyPerformanceMode() => _presetManager.Apply(PerformanceMode.Performance);

    [RelayCommand]
    private void ApplyBalancedMode() => _presetManager.Apply(PerformanceMode.Balanced);

    [RelayCommand]
    private void ApplyQualityMode() => _presetManager.Apply(PerformanceMode.Quality);
}
