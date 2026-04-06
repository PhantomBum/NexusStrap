using System.Reflection;

namespace NexusStrap.Services;

/// <summary>One-time copy of sample mods / plugin docs from the app output folder into the user's data directory.</summary>
public static class BundledContentInitializer
{
    private const string MarkerFileName = ".defaults-seeded-v2";

    public static void SeedIfNeeded(SettingsService settings)
    {
        var marker = Path.Combine(settings.BaseDirectory, MarkerFileName);
        if (File.Exists(marker)) return;

        try
        {
            var appDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var bundledMods = Path.Combine(appDir, "Assets", "Defaults", "Mods");
            var bundledPlugins = Path.Combine(appDir, "Assets", "Defaults", "Plugins");

            CopyTreeIfExists(bundledMods, settings.ModsDirectory);
            CopyTreeIfExists(bundledPlugins, settings.PluginsDirectory);

            File.WriteAllText(marker, Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0");
        }
        catch
        {
            // Non-fatal: user can still add mods manually.
        }
    }

    private static void CopyTreeIfExists(string sourceRoot, string destRoot)
    {
        if (!Directory.Exists(sourceRoot)) return;
        Directory.CreateDirectory(destRoot);

        foreach (var dir in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(sourceRoot, dir);
            var target = Path.Combine(destRoot, rel);
            Directory.CreateDirectory(target);
        }

        foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(sourceRoot, file);
            var target = Path.Combine(destRoot, rel);
            var parent = Path.GetDirectoryName(target);
            if (parent is not null) Directory.CreateDirectory(parent);
            if (!File.Exists(target))
                File.Copy(file, target, overwrite: false);
        }
    }
}
