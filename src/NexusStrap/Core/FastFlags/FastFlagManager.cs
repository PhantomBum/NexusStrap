using System.Text.Json;
using System.Text.Json.Nodes;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.FastFlags;

public sealed class FastFlagManager
{
    private readonly SettingsService _settings;
    private readonly LogService _log;
    private readonly FastFlagHistory _history;

    private Dictionary<string, object?> _currentFlags = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null
    };

    public IReadOnlyDictionary<string, object?> CurrentFlags => _currentFlags;
    public FastFlagHistory History => _history;

    public event Action? FlagsChanged;

    public FastFlagManager(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
        _history = new FastFlagHistory(settings, log);
    }

    public void LoadFlags()
    {
        var versionDir = RegistryManager.GetCurrentRobloxVersionPath();
        if (versionDir is null) return;

        var flagsPath = GetFlagsPath(versionDir);
        if (!File.Exists(flagsPath))
        {
            _currentFlags = new Dictionary<string, object?>();
            return;
        }

        try
        {
            var json = File.ReadAllText(flagsPath);
            var node = JsonNode.Parse(json);
            if (node is JsonObject obj)
            {
                _currentFlags = new Dictionary<string, object?>();
                foreach (var kvp in obj)
                {
                    _currentFlags[kvp.Key] = kvp.Value switch
                    {
                        JsonValue v when v.TryGetValue<bool>(out var b) => b,
                        JsonValue v when v.TryGetValue<int>(out var i) => i,
                        JsonValue v when v.TryGetValue<double>(out var d) => d,
                        JsonValue v when v.TryGetValue<string>(out var s) => s,
                        _ => kvp.Value?.ToString()
                    };
                }
            }
            _log.Info("Loaded {Count} FastFlags", _currentFlags.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load FastFlags");
            _currentFlags = new Dictionary<string, object?>();
        }
    }

    public void SetFlag(string name, object? value)
    {
        _currentFlags[name] = value;
        FlagsChanged?.Invoke();
    }

    public void RemoveFlag(string name)
    {
        _currentFlags.Remove(name);
        FlagsChanged?.Invoke();
    }

    public void ClearFlags()
    {
        _currentFlags.Clear();
        FlagsChanged?.Invoke();
    }

    public void ApplyFlags(string versionDir)
    {
        var flagsPath = GetFlagsPath(versionDir);
        var dir = Path.GetDirectoryName(flagsPath)!;
        Directory.CreateDirectory(dir);

        try
        {
            var json = JsonSerializer.Serialize(_currentFlags, JsonOptions);
            File.WriteAllText(flagsPath, json);
            _log.Info("Applied {Count} FastFlags to {Path}", _currentFlags.Count, flagsPath);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply FastFlags");
        }
    }

    public void ImportFlags(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var imported = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions);
            if (imported is not null)
            {
                foreach (var kvp in imported)
                    _currentFlags[kvp.Key] = kvp.Value;
                FlagsChanged?.Invoke();
                _log.Info("Imported {Count} flags from {Path}", imported.Count, filePath);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to import FastFlags from {Path}", filePath);
        }
    }

    public void ExportFlags(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(_currentFlags, JsonOptions);
            File.WriteAllText(filePath, json);
            _log.Info("Exported {Count} flags to {Path}", _currentFlags.Count, filePath);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to export FastFlags to {Path}", filePath);
        }
    }

    public void ApplyPreset(FastFlagPreset preset)
    {
        _history.SaveSnapshot("Before preset: " + preset.Name, _currentFlags);
        foreach (var kvp in preset.Flags)
            _currentFlags[kvp.Key] = kvp.Value;
        FlagsChanged?.Invoke();
        _log.Info("Applied FastFlag preset: {Name}", preset.Name);
    }

    private static string GetFlagsPath(string versionDir) =>
        Path.Combine(versionDir, "ClientSettings", "ClientAppSettings.json");
}
