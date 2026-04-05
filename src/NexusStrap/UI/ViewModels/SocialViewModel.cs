using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Social;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class SocialViewModel : ObservableObject
{
    private readonly DiscordRichPresenceService _discord;
    private readonly ActivitySharing _activitySharing;
    private readonly FriendTracker _friendTracker;
    private readonly SettingsService _settings;

    [ObservableProperty] private bool _discordEnabled;
    [ObservableProperty] private bool _discordConnected;
    [ObservableProperty] private bool _showGameInRpc;
    [ObservableProperty] private bool _showServerInRpc;
    [ObservableProperty] private string _joinLink = "";

    public SocialViewModel(DiscordRichPresenceService discord, ActivitySharing activitySharing,
        FriendTracker friendTracker, SettingsService settings)
    {
        _discord = discord;
        _activitySharing = activitySharing;
        _friendTracker = friendTracker;
        _settings = settings;

        DiscordEnabled = settings.Settings.EnableDiscordRpc;
        ShowGameInRpc = settings.Settings.ShowGameInRpc;
        ShowServerInRpc = settings.Settings.ShowServerInRpc;
        DiscordConnected = discord.IsConnected;
    }

    partial void OnDiscordEnabledChanged(bool value)
    {
        _settings.Settings.EnableDiscordRpc = value;
        if (value) _discord.Start(); else _discord.Stop();
        DiscordConnected = _discord.IsConnected;
        _settings.SaveSettings();
    }

    partial void OnShowGameInRpcChanged(bool value)
    {
        _settings.Settings.ShowGameInRpc = value;
        _settings.SaveSettings();
    }

    partial void OnShowServerInRpcChanged(bool value)
    {
        _settings.Settings.ShowServerInRpc = value;
        _settings.SaveSettings();
    }

    [RelayCommand]
    private void GenerateJoinLink()
    {
        JoinLink = _activitySharing.GenerateJoinLink() ?? "No active game session";
    }

    [RelayCommand]
    private void CopyJoinLink()
    {
        _activitySharing.CopyJoinLinkToClipboard();
    }

    [RelayCommand]
    private void ConnectDiscord()
    {
        _discord.Start();
        DiscordConnected = _discord.IsConnected;
    }

    [RelayCommand]
    private void DisconnectDiscord()
    {
        _discord.Stop();
        DiscordConnected = false;
    }
}
