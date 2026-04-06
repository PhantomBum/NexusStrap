using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly NotificationService _notifications;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _robloxVersion = "";

    [ObservableProperty]
    private string _appVersion = "NexusStrap";

    public MainWindowViewModel(SettingsService settings, NotificationService notifications)
    {
        _settings = settings;
        _notifications = notifications;

        var v = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = v is null ? "NexusStrap" : $"NexusStrap v{v.Major}.{v.Minor}.{v.Build}";

        RobloxVersion = _settings.RobloxState.InstalledVersionNumber ?? "Roblox: not installed";
        _notifications.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NotificationService.StatusMessage))
                StatusMessage = _notifications.StatusMessage ?? "Ready";
        };
    }
}
