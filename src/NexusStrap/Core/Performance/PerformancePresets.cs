using NexusStrap.Core.FastFlags;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Performance;

public sealed class PerformancePresetManager
{
    private readonly FastFlagManager _fastFlagManager;
    private readonly ProcessPriorityManager _priorityManager;
    private readonly CpuAffinityManager _affinityManager;
    private readonly MemoryManager _memoryManager;
    private readonly BackgroundSuppressor _backgroundSuppressor;
    private readonly SettingsService _settings;
    private readonly LogService _log;

    public PerformancePresetManager(
        FastFlagManager fastFlagManager,
        ProcessPriorityManager priorityManager,
        CpuAffinityManager affinityManager,
        MemoryManager memoryManager,
        BackgroundSuppressor backgroundSuppressor,
        SettingsService settings,
        LogService log)
    {
        _fastFlagManager = fastFlagManager;
        _priorityManager = priorityManager;
        _affinityManager = affinityManager;
        _memoryManager = memoryManager;
        _backgroundSuppressor = backgroundSuppressor;
        _settings = settings;
        _log = log;
    }

    public void Apply(PerformanceMode mode)
    {
        _log.Info("Applying performance preset: {Mode}", mode);

        switch (mode)
        {
            case PerformanceMode.Performance:
                _fastFlagManager.ApplyPreset(FastFlagPresets.FpsBoost);
                _priorityManager.SetRobloxPriority(ProcessPriorityLevel.High);
                _memoryManager.StartPeriodicTrim(30);
                _backgroundSuppressor.Start();
                break;

            case PerformanceMode.Balanced:
                _fastFlagManager.ApplyPreset(FastFlagPresets.Balanced);
                _priorityManager.SetRobloxPriority(ProcessPriorityLevel.AboveNormal);
                _memoryManager.StartPeriodicTrim(60);
                _backgroundSuppressor.Stop();
                break;

            case PerformanceMode.Quality:
                _fastFlagManager.ApplyPreset(FastFlagPresets.VisualQuality);
                _priorityManager.SetRobloxPriority(ProcessPriorityLevel.Normal);
                _memoryManager.StopPeriodicTrim();
                _backgroundSuppressor.Stop();
                break;
        }

        _settings.Settings.PerformanceMode = mode;
        _settings.SaveSettings();
    }
}
