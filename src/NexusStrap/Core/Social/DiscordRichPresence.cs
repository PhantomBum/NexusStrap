using DiscordRPC;
using NexusStrap.Core.Server;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Social;

public sealed class DiscordRichPresenceService : IDisposable
{
    private readonly SettingsService _settings;
    private readonly ServerBrowser _serverBrowser;
    private readonly LogService _log;
    private DiscordRpcClient? _client;
    private bool _isConnected;

    private const string ApplicationId = "1234567890"; // placeholder - register on Discord Dev Portal

    public bool IsConnected => _isConnected;

    public DiscordRichPresenceService(SettingsService settings, ServerBrowser serverBrowser, LogService log)
    {
        _settings = settings;
        _serverBrowser = serverBrowser;
        _log = log;
    }

    public void Start()
    {
        if (!_settings.Settings.EnableDiscordRpc) return;

        try
        {
            _client = new DiscordRpcClient(ApplicationId);
            _client.OnReady += (_, e) =>
            {
                _isConnected = true;
                _log.Info("Discord RPC connected as {User}", e.User?.Username);
            };
            _client.OnError += (_, e) =>
            {
                _log.Warning("Discord RPC error: {Msg}", e.Message);
            };
            _client.Initialize();

            UpdatePresence("In NexusStrap", "Browsing launcher");
            _serverBrowser.ServerChanged += OnServerChanged;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to start Discord RPC");
        }
    }

    public void UpdatePresence(string details, string state, string? largeImageKey = null)
    {
        _client?.SetPresence(new RichPresence
        {
            Details = details,
            State = state,
            Assets = new Assets
            {
                LargeImageKey = largeImageKey ?? "nexusstrap_logo",
                LargeImageText = "NexusStrap"
            },
            Timestamps = Timestamps.Now
        });
    }

    public void ClearPresence()
    {
        _client?.ClearPresence();
    }

    private void OnServerChanged(ServerInfo server)
    {
        if (!_settings.Settings.EnableDiscordRpc) return;

        var details = _settings.Settings.ShowGameInRpc
            ? $"Playing game {server.GameId}"
            : "Playing Roblox";

        var state = _settings.Settings.ShowServerInRpc
            ? $"Server: {server.Region}"
            : "In game";

        UpdatePresence(details, state);
    }

    public void Stop()
    {
        _serverBrowser.ServerChanged -= OnServerChanged;
        ClearPresence();
        _client?.Dispose();
        _client = null;
        _isConnected = false;
    }

    public void Dispose() => Stop();
}
