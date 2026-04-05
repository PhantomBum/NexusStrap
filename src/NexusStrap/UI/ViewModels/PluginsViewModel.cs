using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.PluginHost;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class PluginsViewModel : ObservableObject
{
    private readonly PluginRegistry _registry;
    private readonly PluginLoader _loader;

    [ObservableProperty] private ObservableCollection<PluginInfo> _plugins = new();
    [ObservableProperty] private string _statusText = "";

    public PluginsViewModel(PluginRegistry registry, PluginLoader loader)
    {
        _registry = registry;
        _loader = loader;
        RefreshPlugins();
    }

    private void RefreshPlugins()
    {
        Plugins = new ObservableCollection<PluginInfo>(_registry.DiscoverPlugins());
        StatusText = $"{Plugins.Count} plugins discovered, {Plugins.Count(p => p.IsLoaded)} loaded";
    }

    [RelayCommand]
    private void LoadPlugin(PluginInfo info)
    {
        _loader.LoadPlugin(info.DllPath);
        RefreshPlugins();
    }

    [RelayCommand]
    private void UnloadPlugin(PluginInfo info)
    {
        _loader.UnloadPlugin(info.Id);
        RefreshPlugins();
    }

    [RelayCommand]
    private void RefreshAll() => RefreshPlugins();

    [RelayCommand]
    private void UnloadAll()
    {
        _loader.UnloadAll();
        RefreshPlugins();
    }

    [RelayCommand]
    private void OpenPluginsFolder()
    {
        var dir = _registry.DiscoverPlugins().FirstOrDefault()?.Directory;
        if (dir is not null)
        {
            var parent = System.IO.Path.GetDirectoryName(dir);
            if (parent is not null)
                System.Diagnostics.Process.Start("explorer.exe", parent);
        }
    }
}
