using System.Text.Json;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Macros;

public sealed class KeybindManager
{
    private readonly SettingsService _settings;
    private readonly MacroPlayer _player;
    private readonly LogService _log;
    private readonly List<MacroDefinition> _macros = new();

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public IReadOnlyList<MacroDefinition> Macros => _macros;

    public KeybindManager(SettingsService settings, MacroPlayer player, LogService log)
    {
        _settings = settings;
        _player = player;
        _log = log;
    }

    public void LoadMacros()
    {
        _macros.Clear();
        var dir = _settings.MacrosDirectory;
        if (!Directory.Exists(dir)) return;

        foreach (var file in Directory.GetFiles(dir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var macro = JsonSerializer.Deserialize<MacroDefinition>(json, JsonOpts);
                if (macro is not null) _macros.Add(macro);
            }
            catch (Exception ex)
            {
                _log.Warning("Failed to load macro {File}: {Msg}", file, ex.Message);
            }
        }

        _log.Info("Loaded {Count} macros", _macros.Count);
    }

    public void SaveMacro(MacroDefinition macro)
    {
        var path = Path.Combine(_settings.MacrosDirectory, $"{macro.Id}.json");
        Directory.CreateDirectory(_settings.MacrosDirectory);
        var json = JsonSerializer.Serialize(macro, JsonOpts);
        File.WriteAllText(path, json);

        if (!_macros.Any(m => m.Id == macro.Id))
            _macros.Add(macro);

        _log.Info("Saved macro: {Name}", macro.Name);
    }

    public void DeleteMacro(string macroId)
    {
        var path = Path.Combine(_settings.MacrosDirectory, $"{macroId}.json");
        if (File.Exists(path)) File.Delete(path);
        _macros.RemoveAll(m => m.Id == macroId);
    }

    public IReadOnlyList<MacroDefinition> GetMacrosForGame(string gameId)
    {
        return _macros.Where(m => m.GameId == gameId || m.GameId is null).ToList();
    }
}
