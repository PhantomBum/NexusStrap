using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Core.AI;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly ProtocolHandler _protocolHandler;
    private readonly RegistryManager _registryManager;
    private readonly OptimizationAssistant _optimizer;

    [ObservableProperty] private bool _checkForUpdates;
    [ObservableProperty] private bool _launchOnStartup;
    [ObservableProperty] private bool _minimizeToTray;
    [ObservableProperty] private bool _enableMultiInstance;
    [ObservableProperty] private bool _enablePlugins;
    [ObservableProperty] private bool _enableMods;
    [ObservableProperty] private bool _enableMacros;
    [ObservableProperty] private bool _autoOptimize;
    [ObservableProperty] private int _monitoringInterval;
    [ObservableProperty] private bool _isProtocolRegistered;
    [ObservableProperty] private string _aiRecommendation = "";

    public SettingsViewModel(SettingsService settings, ProtocolHandler protocolHandler,
        RegistryManager registryManager, OptimizationAssistant optimizer)
    {
        _settings = settings;
        _protocolHandler = protocolHandler;
        _registryManager = registryManager;
        _optimizer = optimizer;

        var s = settings.Settings;
        CheckForUpdates = s.CheckForUpdates;
        LaunchOnStartup = s.LaunchOnStartup;
        MinimizeToTray = s.MinimizeToTray;
        EnableMultiInstance = s.EnableMultiInstance;
        EnablePlugins = s.EnablePlugins;
        EnableMods = s.EnableMods;
        EnableMacros = s.EnableMacros;
        AutoOptimize = s.AutoOptimize;
        MonitoringInterval = s.MonitoringIntervalMs;
        IsProtocolRegistered = ProtocolHandler.IsRegistered();
    }

    [RelayCommand]
    private void Save()
    {
        var s = _settings.Settings;
        s.CheckForUpdates = CheckForUpdates;
        s.LaunchOnStartup = LaunchOnStartup;
        s.MinimizeToTray = MinimizeToTray;
        s.EnableMultiInstance = EnableMultiInstance;
        s.EnablePlugins = EnablePlugins;
        s.EnableMods = EnableMods;
        s.EnableMacros = EnableMacros;
        s.AutoOptimize = AutoOptimize;
        s.MonitoringIntervalMs = MonitoringInterval;
        _settings.SaveSettings();
    }

    [RelayCommand]
    private void RegisterProtocol()
    {
        var exePath = Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
        _protocolHandler.RegisterProtocols(exePath);
        _registryManager.RegisterApp(Path.GetDirectoryName(exePath)!, "1.0.0");
        IsProtocolRegistered = true;
    }

    [RelayCommand]
    private void UnregisterProtocol()
    {
        _protocolHandler.UnregisterProtocols();
        _registryManager.UnregisterApp();
        IsProtocolRegistered = false;
    }

    [RelayCommand]
    private void RunAiOptimizer()
    {
        var rec = _optimizer.Analyze();
        AiRecommendation = $"Hardware Tier: {rec.HardwareTier}\nRecommended Mode: {rec.RecommendedMode}\n" +
                          string.Join("\n", rec.Suggestions.Select(s => $"• {s}"));
    }

    [RelayCommand]
    private void ApplyAiOptimization()
    {
        var rec = _optimizer.Analyze();
        _optimizer.ApplyRecommendation(rec);
        AiRecommendation = "Optimization applied!";
    }

    [RelayCommand]
    private void ResetSettings()
    {
        _settings.Settings = new AppSettings();
        _settings.SaveSettings();
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        System.Diagnostics.Process.Start("explorer.exe", _settings.BaseDirectory);
    }
}
