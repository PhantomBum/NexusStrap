using NexusStrap.Services;

namespace NexusStrap.Core.Stability;

public sealed class SafeMode
{
    private readonly SettingsService _settings;
    private readonly LogService _log;

    public bool IsActive { get; private set; }

    public SafeMode(SettingsService settings, LogService log)
    {
        _settings = settings;
        _log = log;
    }

    public void Activate()
    {
        IsActive = true;
        _settings.Settings.SafeMode = true;
        _log.Warning("Safe mode ACTIVATED - all mods, plugins, and customizations disabled");
    }

    public void Deactivate()
    {
        IsActive = false;
        _settings.Settings.SafeMode = false;
        _log.Info("Safe mode deactivated");
    }

    public bool ShouldLoadMods() => !IsActive;
    public bool ShouldLoadPlugins() => !IsActive;
    public bool ShouldApplyCustomTheme() => !IsActive;
    public bool ShouldEnableMacros() => !IsActive;
}
