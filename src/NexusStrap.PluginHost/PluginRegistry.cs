using Microsoft.Extensions.Logging;
using NexusStrap.PluginSDK;

namespace NexusStrap.PluginHost;

public sealed class PluginRegistry
{
    private readonly PluginLoader _loader;
    private readonly ILogger<PluginRegistry> _logger;
    private readonly string _pluginsDirectory;

    public PluginRegistry(PluginLoader loader, ILogger<PluginRegistry> logger, string pluginsDirectory)
    {
        _loader = loader;
        _logger = logger;
        _pluginsDirectory = pluginsDirectory;
    }

    public IReadOnlyList<PluginInfo> DiscoverPlugins()
    {
        var results = new List<PluginInfo>();

        if (!Directory.Exists(_pluginsDirectory))
        {
            Directory.CreateDirectory(_pluginsDirectory);
            return results;
        }

        foreach (var dir in Directory.GetDirectories(_pluginsDirectory))
        {
            var dllFiles = Directory.GetFiles(dir, "*.dll");
            foreach (var dll in dllFiles)
            {
                var name = Path.GetFileNameWithoutExtension(dll);
                var isLoaded = _loader.LoadedPlugins.ContainsKey(name);
                results.Add(new PluginInfo
                {
                    Id = name,
                    DllPath = dll,
                    Directory = dir,
                    IsLoaded = isLoaded,
                    Plugin = isLoaded ? _loader.LoadedPlugins[name].Plugin : null
                });
            }
        }

        return results;
    }

    public async Task LoadAllAsync(IPluginContext context)
    {
        var plugins = DiscoverPlugins();
        foreach (var info in plugins.Where(p => !p.IsLoaded))
        {
            var plugin = _loader.LoadPlugin(info.DllPath);
            if (plugin is not null)
            {
                try
                {
                    await plugin.InitializeAsync(context);
                    await plugin.StartAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize plugin {Id}", info.Id);
                    _loader.UnloadPlugin(info.Id);
                }
            }
        }
    }

    public void UnloadAll() => _loader.UnloadAll();
}

public sealed class PluginInfo
{
    public required string Id { get; init; }
    public required string DllPath { get; init; }
    public required string Directory { get; init; }
    public bool IsLoaded { get; init; }
    public IPlugin? Plugin { get; init; }
}
