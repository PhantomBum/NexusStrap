using System.Text.Json;
using NexusStrap.Models;

namespace NexusStrap.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _baseDir;

    public AppSettings Settings { get; set; } = new();
    public AppState State { get; private set; } = new();
    public RobloxState RobloxState { get; private set; } = new();

    public SettingsService()
    {
        _baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NexusStrap");
        Directory.CreateDirectory(_baseDir);
    }

    public string BaseDirectory => _baseDir;
    public string SettingsPath => Path.Combine(_baseDir, "settings.json");
    public string StatePath => Path.Combine(_baseDir, "state.json");
    public string RobloxStatePath => Path.Combine(_baseDir, "roblox_state.json");
    public string LogsDirectory => Path.Combine(_baseDir, "Logs");
    public string ModsDirectory => Path.Combine(_baseDir, "Mods");
    public string PluginsDirectory => Path.Combine(_baseDir, "Plugins");
    public string ThemesDirectory => Path.Combine(_baseDir, "Themes");
    public string FastFlagPresetsDirectory => Path.Combine(_baseDir, "FastFlagPresets");
    public string MacrosDirectory => Path.Combine(_baseDir, "Macros");

    public void Load()
    {
        Settings = LoadJson<AppSettings>(SettingsPath) ?? new AppSettings();
        State = LoadJson<AppState>(StatePath) ?? new AppState();
        RobloxState = LoadJson<RobloxState>(RobloxStatePath) ?? new RobloxState();
    }

    public void Save()
    {
        SaveJson(SettingsPath, Settings);
        SaveJson(StatePath, State);
        SaveJson(RobloxStatePath, RobloxState);
    }

    public void SaveSettings() => SaveJson(SettingsPath, Settings);
    public void SaveState() => SaveJson(StatePath, State);
    public void SaveRobloxState() => SaveJson(RobloxStatePath, RobloxState);

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(ModsDirectory);
        Directory.CreateDirectory(PluginsDirectory);
        Directory.CreateDirectory(ThemesDirectory);
        Directory.CreateDirectory(FastFlagPresetsDirectory);
        Directory.CreateDirectory(MacrosDirectory);
    }

    private static T? LoadJson<T>(string path) where T : class
    {
        if (!File.Exists(path)) return null;
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveJson<T>(string path, T obj)
    {
        var dir = Path.GetDirectoryName(path);
        if (dir is not null) Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        File.WriteAllText(path, json);
    }
}
