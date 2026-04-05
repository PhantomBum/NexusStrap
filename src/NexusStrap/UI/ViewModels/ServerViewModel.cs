using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.Launch;
using NexusStrap.Core.Server;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class ServerViewModel : ObservableObject
{
    private readonly ServerBrowser _serverBrowser;
    private readonly ServerJoiner _serverJoiner;
    private readonly RegionSelector _regionSelector;
    private readonly PingMonitor _pingMonitor;

    [ObservableProperty] private ServerInfo? _currentServer;
    [ObservableProperty] private string _selectedRegion = "Auto";
    [ObservableProperty] private string _serverStatus = "No active session";
    [ObservableProperty] private string _deepLink = "";
    [ObservableProperty] private string _joinPlaceId = "";

    public IReadOnlyDictionary<string, string> AvailableRegions => RegionSelector.KnownRegions;

    public ServerViewModel(ServerBrowser serverBrowser, ServerJoiner serverJoiner,
        RegionSelector regionSelector, PingMonitor pingMonitor)
    {
        _serverBrowser = serverBrowser;
        _serverJoiner = serverJoiner;
        _regionSelector = regionSelector;
        _pingMonitor = pingMonitor;

        _serverBrowser.ServerChanged += server =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                CurrentServer = server;
                ServerStatus = $"Connected to {server.Region} ({server.IpAddress})";
                DeepLink = _serverBrowser.GenerateDeepLink(server.GameId, server.ServerId);
            });
        };
    }

    [RelayCommand]
    private async Task RefreshServer()
    {
        var server = await _serverBrowser.MonitorLatestLogAsync();
        if (server is not null && server.IpAddress is not null)
        {
            server.Region = await _serverBrowser.DetectRegionAsync(server.IpAddress);
            server.PingMs = await _pingMonitor.PingAsync(server.IpAddress);
        }
    }

    [RelayCommand]
    private async Task JoinServer()
    {
        if (!string.IsNullOrWhiteSpace(JoinPlaceId))
            await _serverJoiner.JoinServerAsync(JoinPlaceId);
    }

    [RelayCommand]
    private async Task ServerHop()
    {
        if (CurrentServer is not null)
            await _serverJoiner.ServerHopAsync(CurrentServer.GameId);
    }

    [RelayCommand]
    private async Task Rejoin()
    {
        await _serverJoiner.RejoinLastAsync();
    }

    [RelayCommand]
    private void CopyDeepLink()
    {
        if (!string.IsNullOrEmpty(DeepLink))
            System.Windows.Clipboard.SetText(DeepLink);
    }

    [RelayCommand]
    private void SetRegion(string region)
    {
        SelectedRegion = region;
        _regionSelector.SetRegion(region);
    }
}
