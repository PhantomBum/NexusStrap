using System.Text.Json;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.FastFlags;

public sealed class FastFlagHistory
{
    private readonly SettingsService _settings;
    private readonly LogService _log;
    private List<FastFlagSnapshot> _snapshots = new();

    private string HistoryPath => Path.Combine(_settings.FastFlagPresetsDirectory, "_history.json");

    public IReadOnlyList<FastFlagSnapshot> Snapshots => _snapshots;

    public FastFlagHistory(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
    }

    public void Load()
    {
        try
        {
            if (File.Exists(HistoryPath))
            {
                var json = File.ReadAllText(HistoryPath);
                _snapshots = JsonSerializer.Deserialize<List<FastFlagSnapshot>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load FastFlag history");
            _snapshots = new();
        }
    }

    public void SaveSnapshot(string label, IReadOnlyDictionary<string, object?> flags)
    {
        var snapshot = new FastFlagSnapshot
        {
            Timestamp = DateTime.Now,
            Label = label,
            Flags = new Dictionary<string, object?>(flags)
        };

        _snapshots.Insert(0, snapshot);
        if (_snapshots.Count > 50)
            _snapshots.RemoveRange(50, _snapshots.Count - 50);

        Save();
        _log.Info("Saved FastFlag snapshot: {Label}", label);
    }

    public Dictionary<string, object?>? GetSnapshot(int index)
    {
        if (index < 0 || index >= _snapshots.Count) return null;
        return new Dictionary<string, object?>(_snapshots[index].Flags);
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_snapshots, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(HistoryPath)!);
            File.WriteAllText(HistoryPath, json);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save FastFlag history");
        }
    }
}
