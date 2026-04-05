namespace NexusStrap.PluginSDK.API;

public interface IUiApi
{
    void ShowNotification(string title, string message, NotificationSeverity severity = NotificationSeverity.Info);
    void ShowStatusMessage(string message);
}

public enum NotificationSeverity
{
    Info,
    Success,
    Warning,
    Error
}
