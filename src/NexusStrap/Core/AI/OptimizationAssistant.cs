using NexusStrap.Core.FastFlags;
using NexusStrap.Core.Performance;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.AI;

public sealed class OptimizationAssistant
{
    private readonly HardwareProfiler _profiler;
    private readonly FastFlagManager _flagManager;
    private readonly SettingsService _settings;
    private readonly LogService _log;

    public OptimizationAssistant(HardwareProfiler profiler, FastFlagManager flagManager,
        SettingsService settings, LogService log)
    {
        _profiler = profiler;
        _flagManager = flagManager;
        _settings = settings;
        _log = log;
    }

    public OptimizationRecommendation Analyze()
    {
        var hw = _profiler.GetProfile();
        var rec = new OptimizationRecommendation { HardwareTier = hw.Tier };

        switch (hw.Tier)
        {
            case HardwareTier.Low:
                rec.RecommendedPreset = FastFlagPresets.Minimal;
                rec.RecommendedMode = PerformanceMode.Performance;
                rec.Suggestions.Add("Use Performance mode for best frame rate");
                rec.Suggestions.Add("Enable memory trimming");
                rec.Suggestions.Add("Disable background process suppression");
                rec.Suggestions.Add("Consider lowering FPS cap to 30-60 for stability");
                break;

            case HardwareTier.Medium:
                rec.RecommendedPreset = FastFlagPresets.FpsBoost;
                rec.RecommendedMode = PerformanceMode.Balanced;
                rec.Suggestions.Add("Balanced mode recommended");
                rec.Suggestions.Add("FPS cap at 60-120 for smooth gameplay");
                rec.Suggestions.Add("Enable periodic memory trimming");
                break;

            case HardwareTier.High:
                rec.RecommendedPreset = FastFlagPresets.Balanced;
                rec.RecommendedMode = PerformanceMode.Balanced;
                rec.Suggestions.Add("Your hardware handles Roblox well");
                rec.Suggestions.Add("Try Quality mode for best visuals");
                rec.Suggestions.Add("Unlimited FPS cap recommended");
                break;

            case HardwareTier.Ultra:
                rec.RecommendedPreset = FastFlagPresets.VisualQuality;
                rec.RecommendedMode = PerformanceMode.Quality;
                rec.Suggestions.Add("Maximum quality recommended");
                rec.Suggestions.Add("Your system can handle all visual features");
                rec.Suggestions.Add("Consider enabling all effects via FastFlags");
                break;
        }

        return rec;
    }

    public void ApplyRecommendation(OptimizationRecommendation rec)
    {
        if (rec.RecommendedPreset is not null)
            _flagManager.ApplyPreset(rec.RecommendedPreset);

        _settings.Settings.PerformanceMode = rec.RecommendedMode;
        _settings.SaveSettings();
        _log.Info("Applied AI optimization for tier {Tier}: {Mode}", rec.HardwareTier, rec.RecommendedMode);
    }
}

public sealed class OptimizationRecommendation
{
    public HardwareTier HardwareTier { get; set; }
    public FastFlagPreset? RecommendedPreset { get; set; }
    public PerformanceMode RecommendedMode { get; set; }
    public List<string> Suggestions { get; set; } = new();
}
