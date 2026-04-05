using System.Text.Json.Serialization;

namespace NexusStrap.Models;

public sealed class AppSettings
{
    // General
    public bool CheckForUpdates { get; set; } = true;
    public bool LaunchOnStartup { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool SafeMode { get; set; }

    // Appearance
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string? CustomThemePath { get; set; }
    public string? CustomBackgroundPath { get; set; }
    public double BackgroundOpacity { get; set; } = 0.3;
    public bool EnableAnimations { get; set; } = true;

    // Performance
    public bool EnableFpsUnlocker { get; set; } = true;
    public int FpsCap { get; set; } = 0; // 0 = unlimited
    public PerformanceMode PerformanceMode { get; set; } = PerformanceMode.Balanced;
    public bool AutoOptimize { get; set; } = true;
    public bool TrimMemory { get; set; } = true;
    public bool SuppressBackground { get; set; }
    public ProcessPriorityLevel RobloxPriority { get; set; } = ProcessPriorityLevel.AboveNormal;

    // Server
    public string PreferredRegion { get; set; } = "Auto";
    public bool AutoJoinLowestPing { get; set; }

    // Multi-Instance
    public bool EnableMultiInstance { get; set; }

    // Mods
    public bool EnableMods { get; set; } = true;

    // Macros
    public bool EnableMacros { get; set; }

    // Social
    public bool EnableDiscordRpc { get; set; } = true;
    public bool ShowGameInRpc { get; set; } = true;
    public bool ShowServerInRpc { get; set; }

    // Monitoring
    public bool EnableOverlay { get; set; }
    public int MonitoringIntervalMs { get; set; } = 500;
    public bool AlertOnFpsDrop { get; set; } = true;
    public int FpsDropThreshold { get; set; } = 30;
    public bool AlertOnHighMemory { get; set; } = true;
    public int HighMemoryThresholdMb { get; set; } = 4096;
    public bool AlertOnPingSpike { get; set; } = true;
    public int PingSpikeThresholdMs { get; set; } = 200;

    // Plugins
    public bool EnablePlugins { get; set; } = true;
    public List<string> DisabledPlugins { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppTheme { Dark, Light, Custom }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PerformanceMode { Performance, Balanced, Quality }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProcessPriorityLevel { Low, BelowNormal, Normal, AboveNormal, High, RealTime }
