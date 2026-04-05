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

    public MainWindowViewModel(SettingsService settings, NotificationService notifications)
    {
        _settings = settings;
        _notifications = notifications;

        RobloxVersion = _settings.RobloxState.InstalledVersionNumber ?? "Not installed";
        _notifications.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NotificationService.StatusMessage))
                StatusMessage = _notifications.StatusMessage ?? "Ready";
        };
    }
}
