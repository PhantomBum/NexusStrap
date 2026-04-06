namespace NexusStrap.Services;

/// <summary>Signals the main window to refresh shell chrome after settings or theme changes.</summary>
public static class ShellBackgroundCoordinator
{
    public static event Action? RefreshRequested;
    public static event Action? AppCursorRefreshRequested;

    public static void RequestRefresh() => RefreshRequested?.Invoke();

    public static void RequestAppCursorRefresh() => AppCursorRefreshRequested?.Invoke();
}
