using System.Diagnostics;
using NexusStrap.Core.Launch;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Server;

public sealed class ServerJoiner
{
    private readonly LaunchController _launchController;
    private readonly ServerBrowser _serverBrowser;
    private readonly LogService _log;

    public ServerJoiner(LaunchController launchController, ServerBrowser serverBrowser, LogService log)
    {
        _launchController = launchController;
        _serverBrowser = serverBrowser;
        _log = log;
    }

    public async Task<bool> JoinServerAsync(string placeId, string? serverInstanceId = null,
        CancellationToken ct = default)
    {
        _log.Info("Joining server: placeId={PlaceId}, instanceId={InstanceId}", placeId, serverInstanceId ?? "auto");
        return await _launchController.LaunchGameAsync(placeId, serverInstanceId, ct);
    }

    public async Task<bool> ServerHopAsync(string placeId, CancellationToken ct = default)
    {
        _log.Info("Server hopping for placeId={PlaceId}", placeId);

        // Kill current Roblox instance
        foreach (var proc in Process.GetProcessesByName("RobloxPlayerBeta"))
        {
            try { proc.Kill(); } catch { }
            proc.Dispose();
        }

        await Task.Delay(1000, ct);
        return await _launchController.LaunchGameAsync(placeId, ct: ct);
    }

    public async Task<bool> RejoinLastAsync(CancellationToken ct = default)
    {
        var current = _serverBrowser.CurrentServer;
        if (current is null)
        {
            _log.Warning("No current server to rejoin");
            return false;
        }

        return await JoinServerAsync(current.GameId, current.ServerId, ct);
    }
}
