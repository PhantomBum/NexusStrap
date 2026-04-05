namespace NexusStrap.Core.Launch;

public sealed class LaunchSettings
{
    public LaunchMode Mode { get; set; } = LaunchMode.Normal;
    public bool IsProtocolLaunch { get; set; }
    public string? ProtocolUri { get; set; }
    public bool SafeMode { get; set; }
    public bool SettingsOnly { get; set; }
    public bool Uninstall { get; set; }
    public bool Silent { get; set; }

    public static LaunchSettings Parse(string[] args)
    {
        var settings = new LaunchSettings();

        foreach (var arg in args)
        {
            if (arg.StartsWith("roblox-player:", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("roblox://", StringComparison.OrdinalIgnoreCase))
            {
                settings.IsProtocolLaunch = true;
                settings.ProtocolUri = arg;
                settings.Mode = LaunchMode.GameLaunch;
            }
            else if (arg.Equals("--settings", StringComparison.OrdinalIgnoreCase))
            {
                settings.SettingsOnly = true;
                settings.Mode = LaunchMode.SettingsOnly;
            }
            else if (arg.Equals("--uninstall", StringComparison.OrdinalIgnoreCase))
            {
                settings.Uninstall = true;
                settings.Mode = LaunchMode.Uninstall;
            }
            else if (arg.Equals("--safemode", StringComparison.OrdinalIgnoreCase))
            {
                settings.SafeMode = true;
            }
            else if (arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
            {
                settings.Silent = true;
            }
        }

        return settings;
    }
}

public enum LaunchMode
{
    Normal,
    GameLaunch,
    SettingsOnly,
    Uninstall
}
