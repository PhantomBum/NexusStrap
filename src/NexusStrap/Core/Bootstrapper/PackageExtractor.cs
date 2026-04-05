using System.IO.Compression;
using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

public sealed class PackageExtractor
{
    private readonly LogService _log;

    // Maps package names to their extraction subdirectories within the version folder
    private static readonly Dictionary<string, string> PackageDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        { "RobloxApp.zip", "" },
        { "shaders.zip", "shaders" },
        { "ssl.zip", "ssl" },
        { "content-avatar.zip", "content\\avatar" },
        { "content-configs.zip", "content\\configs" },
        { "content-fonts.zip", "content\\fonts" },
        { "content-sky.zip", "content\\sky" },
        { "content-sounds.zip", "content\\sounds" },
        { "content-textures2.zip", "content\\textures" },
        { "content-models.zip", "content\\models" },
        { "content-textures3.zip", "PlatformContent\\pc\\textures" },
        { "content-terrain.zip", "PlatformContent\\pc\\terrain" },
        { "content-platform-fonts.zip", "PlatformContent\\pc\\fonts" },
        { "extracontent-luapackages.zip", "ExtraContent\\LuaPackages" },
        { "extracontent-translations.zip", "ExtraContent\\translations" },
        { "extracontent-models.zip", "ExtraContent\\models" },
        { "extracontent-textures.zip", "ExtraContent\\textures" },
        { "extracontent-places.zip", "ExtraContent\\places" },
        { "WebView2.zip", "" },
        { "WebView2RuntimeInstaller.zip", "" }
    };

    public PackageExtractor(LogService log)
    {
        _log = log;
    }

    public async Task ExtractAllAsync(string downloadDir, string versionDir,
        PackageManifest manifest, IProgress<(string Package, double Progress)>? progress = null,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(versionDir);

        for (int i = 0; i < manifest.Packages.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var pkg = manifest.Packages[i];
            var zipPath = Path.Combine(downloadDir, pkg.Name);

            if (!File.Exists(zipPath))
            {
                _log.Warning("Package file missing, skipping: {Name}", pkg.Name);
                continue;
            }

            if (ShouldSkipPackage(pkg.Name))
            {
                _log.Debug("Skipping conditional package: {Name}", pkg.Name);
                continue;
            }

            var targetDir = GetExtractionDirectory(versionDir, pkg.Name);
            Directory.CreateDirectory(targetDir);

            _log.Info("Extracting {Name} to {Dir}", pkg.Name, targetDir);

            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(zipPath, targetDir, overwriteFiles: true);
            }, ct);

            progress?.Report((pkg.Name, (double)(i + 1) / manifest.Packages.Count));
        }
    }

    private string GetExtractionDirectory(string versionDir, string packageName)
    {
        if (PackageDirectories.TryGetValue(packageName, out var subDir) && !string.IsNullOrEmpty(subDir))
            return Path.Combine(versionDir, subDir);
        return versionDir;
    }

    private bool ShouldSkipPackage(string packageName)
    {
        // WebView2RuntimeInstaller.zip only needs extraction if WebView2 isn't installed
        if (packageName.Equals("WebView2RuntimeInstaller.zip", StringComparison.OrdinalIgnoreCase))
        {
            return IsWebView2Installed();
        }
        return false;
    }

    private bool IsWebView2Installed()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            return key?.GetValue("pv") is not null;
        }
        catch
        {
            return false;
        }
    }
}
