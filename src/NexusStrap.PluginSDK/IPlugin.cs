namespace NexusStrap.PluginSDK;

public interface IPlugin : IDisposable
{
    string Name { get; }
    string Description { get; }
    string Author { get; }
    Version Version { get; }

    Task InitializeAsync(IPluginContext context);
    Task StartAsync();
    Task StopAsync();
}
