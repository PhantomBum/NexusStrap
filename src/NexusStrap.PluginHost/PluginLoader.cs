using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using NexusStrap.PluginSDK;

namespace NexusStrap.PluginHost;

public sealed class PluginLoader
{
    private readonly ILogger<PluginLoader> _logger;
    private readonly Dictionary<string, PluginSandbox> _sandboxes = new();

    public PluginLoader(ILogger<PluginLoader> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, PluginSandbox> LoadedPlugins => _sandboxes;

    public IPlugin? LoadPlugin(string pluginPath)
    {
        try
        {
            var dllPath = Path.GetFullPath(pluginPath);
            if (!File.Exists(dllPath))
            {
                _logger.LogWarning("Plugin DLL not found: {Path}", dllPath);
                return null;
            }

            var pluginId = Path.GetFileNameWithoutExtension(dllPath);
            if (_sandboxes.ContainsKey(pluginId))
            {
                _logger.LogWarning("Plugin already loaded: {Id}", pluginId);
                return _sandboxes[pluginId].Plugin;
            }

            var sandbox = new PluginSandbox(dllPath);
            var plugin = sandbox.LoadAndCreate();

            if (plugin is null)
            {
                _logger.LogWarning("No IPlugin implementation found in {Path}", dllPath);
                sandbox.Unload();
                return null;
            }

            _sandboxes[pluginId] = sandbox;
            _logger.LogInformation("Loaded plugin: {Name} v{Version} by {Author}",
                plugin.Name, plugin.Version, plugin.Author);
            return plugin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load plugin from {Path}", pluginPath);
            return null;
        }
    }

    public bool UnloadPlugin(string pluginId)
    {
        if (!_sandboxes.TryGetValue(pluginId, out var sandbox))
            return false;

        try
        {
            sandbox.Plugin?.StopAsync().GetAwaiter().GetResult();
            sandbox.Plugin?.Dispose();
            sandbox.Unload();
            _sandboxes.Remove(pluginId);
            _logger.LogInformation("Unloaded plugin: {Id}", pluginId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unloading plugin {Id}", pluginId);
            return false;
        }
    }

    public async Task<IPlugin?> ReloadPluginAsync(string pluginPath)
    {
        var pluginId = Path.GetFileNameWithoutExtension(pluginPath);
        UnloadPlugin(pluginId);
        await Task.Delay(100); // allow GC to collect
        return LoadPlugin(pluginPath);
    }

    public void UnloadAll()
    {
        foreach (var id in _sandboxes.Keys.ToList())
            UnloadPlugin(id);
    }
}
