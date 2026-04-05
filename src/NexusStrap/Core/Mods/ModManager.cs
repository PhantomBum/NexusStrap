using System.Text.Json;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Mods;

public sealed class ModManager
{
    private readonly SettingsService _settings;
    private readonly LogService _log;
    private readonly ConflictDetector _conflictDetector;
    private readonly List<ModInfo> _mods = new();

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public IReadOnlyList<ModInfo> Mods => _mods;
    public event Action? ModsChanged;

    public ModManager(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
        _conflictDetector = new ConflictDetector();
    }

    public void LoadMods()
    {
        _mods.Clear();
        var modsDir = _settings.ModsDirectory;
        if (!Directory.Exists(modsDir)) return;

        foreach (var dir in Directory.GetDirectories(modsDir))
        {
            var manifestPath = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            try
            {
                var json = File.ReadAllText(manifestPath);
                var mod = JsonSerializer.Deserialize<ModInfo>(json, JsonOpts);
                if (mod is not null)
                {
                    mod.DirectoryPath = dir;
                    _mods.Add(mod);
                }
            }
            catch (Exception ex)
            {
                _log.Warning("Failed to load mod from {Dir}: {Msg}", dir, ex.Message);
            }
        }

        _log.Info("Loaded {Count} mods", _mods.Count);
    }

    public void ApplyEnabledMods()
    {
        var versionDir = RegistryManager.GetCurrentRobloxVersionPath();
        if (versionDir is null) return;

        foreach (var mod in _mods.Where(m => m.IsEnabled))
        {
            ApplyMod(mod, versionDir);
        }
    }

    public void EnableMod(string modId)
    {
        var mod = _mods.FirstOrDefault(m => m.Id == modId);
        if (mod is null) return;

        mod.IsEnabled = true;
        SaveModManifest(mod);
        ModsChanged?.Invoke();
        _log.Info("Enabled mod: {Name}", mod.Name);
    }

    public void DisableMod(string modId)
    {
        var mod = _mods.FirstOrDefault(m => m.Id == modId);
        if (mod is null) return;

        mod.IsEnabled = false;

        var versionDir = RegistryManager.GetCurrentRobloxVersionPath();
        if (versionDir is not null) RemoveModFiles(mod, versionDir);

        SaveModManifest(mod);
        ModsChanged?.Invoke();
        _log.Info("Disabled mod: {Name}", mod.Name);
    }

    public IReadOnlyList<ModConflict> CheckConflicts() => _conflictDetector.Detect(_mods);

    private void ApplyMod(ModInfo mod, string robloxDir)
    {
        foreach (var mapping in mod.FileMappings)
        {
            var source = Path.Combine(mod.DirectoryPath, mapping.Source);
            var target = Path.Combine(robloxDir, mapping.Target);

            if (!File.Exists(source)) continue;

            try
            {
                var dir = Path.GetDirectoryName(target);
                if (dir is not null) Directory.CreateDirectory(dir);
                File.Copy(source, target, overwrite: true);
            }
            catch (Exception ex)
            {
                _log.Warning("Failed to apply mod file {Source} -> {Target}: {Msg}", source, target, ex.Message);
            }
        }
    }

    private void RemoveModFiles(ModInfo mod, string robloxDir)
    {
        foreach (var mapping in mod.FileMappings)
        {
            var target = Path.Combine(robloxDir, mapping.Target);
            try
            {
                if (File.Exists(target)) File.Delete(target);
            }
            catch { }
        }
    }

    private void SaveModManifest(ModInfo mod)
    {
        var path = Path.Combine(mod.DirectoryPath, "manifest.json");
        var json = JsonSerializer.Serialize(mod, JsonOpts);
        File.WriteAllText(path, json);
    }
}
