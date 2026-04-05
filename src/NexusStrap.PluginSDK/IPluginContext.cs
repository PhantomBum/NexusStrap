using NexusStrap.PluginSDK.API;

namespace NexusStrap.PluginSDK;

public interface IPluginContext
{
    ITelemetryApi Telemetry { get; }
    IUiApi Ui { get; }
    ISettingsApi Settings { get; }

    string PluginDataDirectory { get; }

    void SubscribeEvent(string eventName, Action<object?> handler);
    void UnsubscribeEvent(string eventName, Action<object?> handler);
}
