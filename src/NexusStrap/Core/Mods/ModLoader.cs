using System.IO.Compression;
using System.Text.Json;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Mods;

public sealed class ModLoader
{
    private readonly SettingsService _settings;
    private readonly LogService _log;

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ModLoader(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
    }

    public ModInfo? InstallFromZip(string zipPath)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "NexusStrap_mod_" + Guid.NewGuid().ToString("N")[..8]);
            ZipFile.ExtractToDirectory(zipPath, tempDir);

            var manifestPath = Path.Combine(tempDir, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                _log.Warning("Mod zip does not contain manifest.json");
                Directory.Delete(tempDir, true);
                return null;
            }

            var json = File.ReadAllText(manifestPath);
            var mod = JsonSerializer.Deserialize<ModInfo>(json, JsonOpts);
            if (mod is null)
            {
                Directory.Delete(tempDir, true);
                return null;
            }

            var targetDir = Path.Combine(_settings.ModsDirectory, mod.Id);
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);

            Directory.Move(tempDir, targetDir);
            mod.DirectoryPath = targetDir;
            _log.Info("Installed mod: {Name} to {Dir}", mod.Name, targetDir);
            return mod;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to install mod from {Path}", zipPath);
            return null;
        }
    }

    public bool UninstallMod(ModInfo mod)
    {
        try
        {
            if (Directory.Exists(mod.DirectoryPath))
            {
                Directory.Delete(mod.DirectoryPath, true);
                _log.Info("Uninstalled mod: {Name}", mod.Name);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to uninstall mod {Name}", mod.Name);
            return false;
        }
    }

    public ModInfo CreateMod(string name, string author, string description)
    {
        var mod = new ModInfo
        {
            Id = name.ToLowerInvariant().Replace(' ', '-'),
            Name = name,
            Author = author,
            Description = description,
            DirectoryPath = Path.Combine(_settings.ModsDirectory, name.ToLowerInvariant().Replace(' ', '-'))
        };

        Directory.CreateDirectory(mod.DirectoryPath);
        var json = JsonSerializer.Serialize(mod, JsonOpts);
        File.WriteAllText(Path.Combine(mod.DirectoryPath, "manifest.json"), json);
        return mod;
    }
}
