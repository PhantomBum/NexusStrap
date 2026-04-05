using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Monitoring;

public sealed class AlertEngine
{
    private readonly SettingsService _settings;
    private readonly NotificationService _notifications;
    private readonly EventBus _eventBus;

    private DateTime _lastFpsAlert = DateTime.MinValue;
    private DateTime _lastMemoryAlert = DateTime.MinValue;
    private DateTime _lastPingAlert = DateTime.MinValue;

    private static readonly TimeSpan AlertCooldown = TimeSpan.FromSeconds(30);

    public AlertEngine(SettingsService settings, NotificationService notifications, EventBus eventBus)
    {
        _settings = settings;
        _notifications = notifications;
        _eventBus = eventBus;
    }

    public void Evaluate(PerformanceSnapshot snapshot)
    {
        var s = _settings.Settings;
        var now = DateTime.Now;

        if (s.AlertOnFpsDrop && snapshot.Fps > 0 && snapshot.Fps < s.FpsDropThreshold &&
            now - _lastFpsAlert > AlertCooldown)
        {
            _notifications.Show("FPS Drop", $"FPS dropped to {snapshot.Fps:F0}", NotificationLevel.Warning);
            _eventBus.Publish(PluginSDK.Events.GameEvents.OnFpsDrop, snapshot.Fps);
            _lastFpsAlert = now;
        }

        if (s.AlertOnHighMemory && snapshot.RamUsageMb > s.HighMemoryThresholdMb &&
            now - _lastMemoryAlert > AlertCooldown)
        {
            _notifications.Show("High Memory", $"RAM usage: {snapshot.RamUsageMb} MB", NotificationLevel.Warning);
            _eventBus.Publish(PluginSDK.Events.GameEvents.OnHighMemory, snapshot.RamUsageMb);
            _lastMemoryAlert = now;
        }

        if (s.AlertOnPingSpike && snapshot.PingMs > s.PingSpikeThresholdMs &&
            now - _lastPingAlert > AlertCooldown)
        {
            _notifications.Show("Ping Spike", $"Ping: {snapshot.PingMs:F0} ms", NotificationLevel.Warning);
            _eventBus.Publish(PluginSDK.Events.GameEvents.OnPingSpike, snapshot.PingMs);
            _lastPingAlert = now;
        }
    }
}
