using NexusStrap.Core.Bootstrapper;
using NexusStrap.Services;

namespace NexusStrap.Core.Launch;

public sealed class LaunchController
{
    private readonly RobloxBootstrapper _bootstrapper;
    private readonly ProtocolHandler _protocolHandler;
    private readonly SettingsService _settings;
    private readonly LogService _log;

    public LaunchController(
        RobloxBootstrapper bootstrapper,
        ProtocolHandler protocolHandler,
        SettingsService settings,
        LogService log)
    {
        _bootstrapper = bootstrapper;
        _protocolHandler = protocolHandler;
        _settings = settings;
        _log = log;
    }

    public async Task<bool> HandleLaunchAsync(LaunchSettings launchSettings, CancellationToken ct = default)
    {
        switch (launchSettings.Mode)
        {
            case LaunchMode.GameLaunch:
                return await _bootstrapper.RunAsync(launchSettings.ProtocolUri, ct);

            case LaunchMode.Normal:
                // Just open the UI, no game launch
                return true;

            case LaunchMode.SettingsOnly:
                return true;

            case LaunchMode.Uninstall:
                PerformUninstall();
                return true;

            default:
                return true;
        }
    }

    public async Task<bool> LaunchGameAsync(string? placeId = null, string? serverInstanceId = null,
        CancellationToken ct = default)
    {
        string launchUri;
        if (placeId is not null && serverInstanceId is not null)
        {
            launchUri = $"roblox://experiences/start?placeId={placeId}&gameInstanceId={serverInstanceId}";
        }
        else if (placeId is not null)
        {
            launchUri = $"roblox://experiences/start?placeId={placeId}";
        }
        else
        {
            launchUri = "roblox-player:1+launchmode:app";
        }

        return await _bootstrapper.RunAsync(launchUri, ct);
    }

    public async Task<bool> LaunchDesktopAppAsync(CancellationToken ct = default)
    {
        return await _bootstrapper.RunAsync("roblox-player:1+launchmode:app", ct);
    }

    private void PerformUninstall()
    {
        _log.Info("Performing NexusStrap uninstall");
        _protocolHandler.UnregisterProtocols();
        new RegistryManager(_log).UnregisterApp();
        _log.Info("Uninstall complete");
    }
}
