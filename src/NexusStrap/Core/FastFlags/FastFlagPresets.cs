using NexusStrap.Models;

namespace NexusStrap.Core.FastFlags;

public static class FastFlagPresets
{
    public static IReadOnlyList<FastFlagPreset> BuiltInPresets { get; } = new List<FastFlagPreset>
    {
        FpsBoost,
        LowLatency,
        VisualQuality,
        Minimal,
        Balanced
    };

    public static FastFlagPreset FpsBoost { get; } = new()
    {
        Name = "FPS Boost",
        Description = "Maximizes frame rate by reducing visual quality",
        Category = "Performance",
        Flags = new Dictionary<string, object>
        {
            ["DFIntTaskSchedulerTargetFps"] = 9999,
            ["FFlagDebugGraphicsPreferVulkan"] = true,
            ["FIntRenderGrassDetailStrands"] = 0,
            ["FIntRenderShadowIntensity"] = 0,
            ["FIntTerrainOctreeMaxDepth"] = 3,
            ["FFlagGlobalWindRendering"] = false,
            ["FIntRenderLocalLightUpdatesMax"] = 1,
            ["FIntRenderLocalLightUpdatesMin"] = 1,
            ["DFFlagDebugRenderForceTechnologyVoxel"] = true
        }
    };

    public static FastFlagPreset LowLatency { get; } = new()
    {
        Name = "Low Latency",
        Description = "Minimizes input lag and network latency",
        Category = "Performance",
        Flags = new Dictionary<string, object>
        {
            ["DFIntTaskSchedulerTargetFps"] = 9999,
            ["FFlagGameBasicSettingsFramerateCap"] = false,
            ["DFFlagDebugPauseVoxelizer"] = true
        }
    };

    public static FastFlagPreset VisualQuality { get; } = new()
    {
        Name = "Visual Quality",
        Description = "Maximum visual fidelity",
        Category = "Quality",
        Flags = new Dictionary<string, object>
        {
            ["FIntRenderShadowIntensity"] = 100,
            ["FIntTerrainOctreeMaxDepth"] = 8,
            ["FFlagGlobalWindRendering"] = true,
            ["FIntRenderLocalLightUpdatesMax"] = 8,
            ["FIntRenderLocalLightUpdatesMin"] = 4,
            ["FIntRenderGrassDetailStrands"] = 100
        }
    };

    public static FastFlagPreset Minimal { get; } = new()
    {
        Name = "Minimal",
        Description = "Absolute minimum graphics for lowest-end hardware",
        Category = "Performance",
        Flags = new Dictionary<string, object>
        {
            ["DFIntTaskSchedulerTargetFps"] = 9999,
            ["FIntRenderGrassDetailStrands"] = 0,
            ["FIntRenderShadowIntensity"] = 0,
            ["FIntTerrainOctreeMaxDepth"] = 2,
            ["FFlagGlobalWindRendering"] = false,
            ["FIntRenderLocalLightUpdatesMax"] = 0,
            ["FIntRenderLocalLightUpdatesMin"] = 0,
            ["DFFlagDebugRenderForceTechnologyVoxel"] = true,
            ["FFlagDebugSkyGray"] = true
        }
    };

    public static FastFlagPreset Balanced { get; } = new()
    {
        Name = "Balanced",
        Description = "Good balance between performance and quality",
        Category = "General",
        Flags = new Dictionary<string, object>
        {
            ["DFIntTaskSchedulerTargetFps"] = 9999,
            ["FIntRenderShadowIntensity"] = 50,
            ["FIntTerrainOctreeMaxDepth"] = 5,
            ["FIntRenderLocalLightUpdatesMax"] = 4,
            ["FIntRenderLocalLightUpdatesMin"] = 2,
            ["FFlagGlobalWindRendering"] = true,
            ["FIntRenderGrassDetailStrands"] = 50
        }
    };
}
