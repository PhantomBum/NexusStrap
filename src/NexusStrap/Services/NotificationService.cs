using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NexusStrap.Services;

public sealed class NotificationService : ObservableObject
{
    public ObservableCollection<AppNotification> Notifications { get; } = new();

    private string? _statusMessage;
    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public void Show(string title, string message, NotificationLevel level = NotificationLevel.Info)
    {
        var notification = new AppNotification
        {
            Title = title,
            Message = message,
            Level = level,
            Timestamp = DateTime.Now
        };

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, notification);
            if (Notifications.Count > 50)
                Notifications.RemoveAt(Notifications.Count - 1);
        });
    }

    public void ShowStatus(string message)
    {
        StatusMessage = message;
    }

    public void ClearStatus() => StatusMessage = null;
}

public sealed class AppNotification
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationLevel Level { get; init; }
    public DateTime Timestamp { get; init; }
    public bool IsRead { get; set; }
}

public enum NotificationLevel { Info, Success, Warning, Error }
