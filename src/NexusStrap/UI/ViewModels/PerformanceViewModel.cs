using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Performance;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class PerformanceViewModel : ObservableObject
{
    private readonly FpsUnlocker _fpsUnlocker;
    private readonly MemoryManager _memoryManager;
    private readonly CpuAffinityManager _affinityManager;
    private readonly ProcessPriorityManager _priorityManager;
    private readonly BackgroundSuppressor _suppressor;
    private readonly SettingsService _settings;

    [ObservableProperty] private bool _fpsUnlockerEnabled;
    [ObservableProperty] private int _fpsCap;
    [ObservableProperty] private bool _memoryTrimEnabled;
    [ObservableProperty] private bool _backgroundSuppressionEnabled;
    [ObservableProperty] private ProcessPriorityLevel _selectedPriority;
    [ObservableProperty] private long _robloxMemoryMb;
    [ObservableProperty] private int _processorCount;

    public PerformanceViewModel(FpsUnlocker fpsUnlocker, MemoryManager memoryManager,
        CpuAffinityManager affinityManager, ProcessPriorityManager priorityManager,
        BackgroundSuppressor suppressor, SettingsService settings)
    {
        _fpsUnlocker = fpsUnlocker;
        _memoryManager = memoryManager;
        _affinityManager = affinityManager;
        _priorityManager = priorityManager;
        _suppressor = suppressor;
        _settings = settings;

        FpsUnlockerEnabled = settings.Settings.EnableFpsUnlocker;
        FpsCap = settings.Settings.FpsCap;
        MemoryTrimEnabled = settings.Settings.TrimMemory;
        BackgroundSuppressionEnabled = settings.Settings.SuppressBackground;
        SelectedPriority = settings.Settings.RobloxPriority;
        ProcessorCount = Environment.ProcessorCount;
        RobloxMemoryMb = MemoryManager.GetRobloxMemoryUsageMb();
    }

    partial void OnFpsUnlockerEnabledChanged(bool value)
    {
        _settings.Settings.EnableFpsUnlocker = value;
        if (value) _fpsUnlocker.Start(FpsCap); else _fpsUnlocker.Stop();
        _settings.SaveSettings();
    }

    partial void OnFpsCapChanged(int value)
    {
        _settings.Settings.FpsCap = value;
        _fpsUnlocker.SetTargetFps(value);
        _settings.SaveSettings();
    }

    partial void OnMemoryTrimEnabledChanged(bool value)
    {
        _settings.Settings.TrimMemory = value;
        if (value) _memoryManager.StartPeriodicTrim(); else _memoryManager.StopPeriodicTrim();
        _settings.SaveSettings();
    }

    partial void OnBackgroundSuppressionEnabledChanged(bool value)
    {
        _settings.Settings.SuppressBackground = value;
        if (value) _suppressor.Start(); else _suppressor.Stop();
        _settings.SaveSettings();
    }

    [RelayCommand]
    private void TrimMemoryNow()
    {
        _memoryManager.TrimRobloxMemory();
        RobloxMemoryMb = MemoryManager.GetRobloxMemoryUsageMb();
    }

    [RelayCommand]
    private void SetPriority(ProcessPriorityLevel level)
    {
        _priorityManager.SetRobloxPriority(level);
        SelectedPriority = level;
        _settings.Settings.RobloxPriority = level;
        _settings.SaveSettings();
    }

    [RelayCommand]
    private void ResetAffinity() => _affinityManager.ResetRobloxAffinity();
}
