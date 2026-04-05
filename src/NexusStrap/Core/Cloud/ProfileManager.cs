using System.Text.Json;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Cloud;

public sealed class ProfileManager
{
    private readonly SettingsService _settings;
    private readonly LogService _log;
    private readonly Dictionary<string, GameProfile> _profiles = new();

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public IReadOnlyDictionary<string, GameProfile> Profiles => _profiles;

    public ProfileManager(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
    }

    public void Load()
    {
        var profilesDir = Path.Combine(_settings.BaseDirectory, "Profiles");
        if (!Directory.Exists(profilesDir)) return;

        foreach (var file in Directory.GetFiles(profilesDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var profile = JsonSerializer.Deserialize<GameProfile>(json, JsonOpts);
                if (profile is not null)
                    _profiles[profile.GameId] = profile;
            }
            catch { }
        }
    }

    public GameProfile GetOrCreate(string gameId)
    {
        if (_profiles.TryGetValue(gameId, out var existing)) return existing;

        var profile = new GameProfile { GameId = gameId };
        _profiles[gameId] = profile;
        return profile;
    }

    public void Save(GameProfile profile)
    {
        var dir = Path.Combine(_settings.BaseDirectory, "Profiles");
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(profile, JsonOpts);
        File.WriteAllText(Path.Combine(dir, $"{profile.GameId}.json"), json);
        _profiles[profile.GameId] = profile;
    }
}

public sealed class GameProfile
{
    public string GameId { get; set; } = string.Empty;
    public string? GameName { get; set; }
    public PerformanceMode? PreferredMode { get; set; }
    public Dictionary<string, object?> CustomFlags { get; set; } = new();
    public string? PreferredRegion { get; set; }
    public List<string> EnabledMacroIds { get; set; } = new();
}
