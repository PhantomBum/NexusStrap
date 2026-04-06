using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

/// <summary>
/// Copies a user-selected PNG into the Roblox player content tree so the client loads it as the default arrow cursor.
/// Path matches Bloxstrap-style overlays: <c>content/textures/Cursors/KeyboardMouse/ArrowCursor.png</c>.
/// </summary>
public static class RobloxCursorInstaller
{
    private const string ArrowRelative = @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png";
    private const string ArrowFarRelative = @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png";

    public static void ApplyCustomCursor(SettingsService settings, string versionDir, LogService log)
    {
        var s = settings.Settings;
        if (!s.EnableCustomRobloxCursor || string.IsNullOrWhiteSpace(s.CustomRobloxCursorPath))
            return;

        var src = s.CustomRobloxCursorPath;
        if (!File.Exists(src))
        {
            log.Warning("Custom Roblox cursor file not found: {Path}", src);
            return;
        }

        try
        {
            CopyTo(src, Path.Combine(versionDir, ArrowRelative));
            CopyTo(src, Path.Combine(versionDir, ArrowFarRelative));
            log.Info("Installed custom Roblox cursor textures from {Src}", src);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to install custom Roblox cursor");
        }
    }

    private static void CopyTo(string sourceFile, string destinationPath)
    {
        var dir = Path.GetDirectoryName(destinationPath);
        if (dir is not null) Directory.CreateDirectory(dir);
        File.Copy(sourceFile, destinationPath, overwrite: true);
    }
}
