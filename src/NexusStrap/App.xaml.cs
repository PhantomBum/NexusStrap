using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusStrap.Core.AI;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Core.Cloud;
using NexusStrap.Core.FastFlags;
using NexusStrap.Core.Launch;
using NexusStrap.Core.Macros;
using NexusStrap.Core.Mods;
using NexusStrap.Core.Monitoring;
using NexusStrap.Core.Performance;
using NexusStrap.Core.Server;
using NexusStrap.Core.Social;
using NexusStrap.Core.Stability;
using NexusStrap.Core.Utilities;
using NexusStrap.PluginHost;
using NexusStrap.Services;
using NexusStrap.UI.ViewModels;
using NexusStrap.UI.Views;
using NexusStrap.UI.Views.Pages;

namespace NexusStrap;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            RunStartup(e);
        }
        catch (Exception ex)
        {
            StartupDiagnostics.WriteFatal(ex);
            Shutdown(-1);
        }
    }

    private void RunStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        var settingsService = Services.GetRequiredService<SettingsService>();
        settingsService.Load();
        settingsService.EnsureDirectories();

        var crashLogger = Services.GetRequiredService<CrashLogger>();
        crashLogger.RegisterGlobalHandlers();

        var themeService = Services.GetRequiredService<ThemeService>();
        themeService.ApplyTheme(settingsService.Settings.Theme);

        var launchSettings = Core.Launch.LaunchSettings.Parse(e.Args);

        if (launchSettings.SafeMode)
        {
            Services.GetRequiredService<SafeMode>().Activate();
        }

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        if (launchSettings.IsProtocolLaunch)
        {
            _ = Task.Run(async () =>
            {
                var controller = Services.GetRequiredService<LaunchController>();
                await controller.HandleLaunchAsync(launchSettings);
            });
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<HttpService>();
        services.AddSingleton<EventBus>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton(sp => new LogService(sp.GetRequiredService<SettingsService>().LogsDirectory));
        services.AddSingleton<ThemeService>();

        // Bootstrapper
        services.AddSingleton<VersionChecker>();
        services.AddSingleton<PackageDownloader>();
        services.AddSingleton<PackageExtractor>();
        services.AddSingleton<ProtocolHandler>();
        services.AddSingleton<RegistryManager>();
        services.AddSingleton<RobloxBootstrapper>();

        // Launch
        services.AddSingleton<LaunchController>();
        services.AddSingleton<MultiInstanceManager>();

        // Performance
        services.AddSingleton<FpsUnlocker>();
        services.AddSingleton<MemoryManager>();
        services.AddSingleton<CpuAffinityManager>();
        services.AddSingleton<ProcessPriorityManager>();
        services.AddSingleton<BackgroundSuppressor>();
        services.AddSingleton<PerformancePresetManager>();

        // FastFlags
        services.AddSingleton<FastFlagManager>();

        // Server
        services.AddSingleton<PingMonitor>();
        services.AddSingleton<ServerBrowser>();
        services.AddSingleton<RegionSelector>();
        services.AddSingleton<ServerJoiner>();

        // Monitoring
        services.AddSingleton<SystemMetrics>();
        services.AddSingleton<AlertEngine>();
        services.AddSingleton<BottleneckDetector>();
        services.AddSingleton<ResourceMonitor>();

        // Mods
        services.AddSingleton<ModManager>();
        services.AddSingleton<ModLoader>();

        // Macros
        services.AddSingleton<MacroRecorder>();
        services.AddSingleton<MacroPlayer>();
        services.AddSingleton<KeybindManager>();
        services.AddSingleton<TriggerEngine>();

        // Utilities
        services.AddSingleton<SystemCleaner>();
        services.AddSingleton<InstallManager>();
        services.AddSingleton<VersionManager>();

        // Stability
        services.AddSingleton<CrashLogger>();
        services.AddSingleton<DiagnosticsEngine>();
        services.AddSingleton<SafeMode>();
        services.AddSingleton<DebugOverlay>();

        // Social
        services.AddSingleton<DiscordRichPresenceService>();
        services.AddSingleton<FriendTracker>();
        services.AddSingleton<ActivitySharing>();

        // AI
        services.AddSingleton<HardwareProfiler>();
        services.AddSingleton<OptimizationAssistant>();
        services.AddSingleton<PredictiveTuner>();

        // Cloud
        services.AddSingleton<ICloudSyncService, LocalCloudSyncStub>();
        services.AddSingleton<ProfileManager>();

        // Plugins
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<PluginLoader>>(sp =>
        {
            var factory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            return factory.CreateLogger<PluginLoader>();
        });
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<PluginRegistry>>(sp =>
        {
            var factory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            return factory.CreateLogger<PluginRegistry>();
        });
        services.AddSingleton<PluginLoader>();
        services.AddSingleton(sp => new PluginRegistry(
            sp.GetRequiredService<PluginLoader>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PluginRegistry>>(),
            sp.GetRequiredService<SettingsService>().PluginsDirectory));

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PerformanceViewModel>();
        services.AddTransient<ServerViewModel>();
        services.AddTransient<FastFlagViewModel>();
        services.AddTransient<ModsViewModel>();
        services.AddTransient<PluginsViewModel>();
        services.AddTransient<MacrosViewModel>();
        services.AddTransient<CustomizationViewModel>();
        services.AddTransient<MonitorViewModel>();
        services.AddTransient<UtilitiesViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<StabilityViewModel>();
        services.AddTransient<SocialViewModel>();

        // Views — MainWindow + all navigation pages (WPF-UI NavigationView resolves pages via IServiceProvider)
        services.AddSingleton<MainWindow>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<PerformancePage>();
        services.AddTransient<ServerPage>();
        services.AddTransient<FastFlagPage>();
        services.AddTransient<ModsPage>();
        services.AddTransient<PluginsPage>();
        services.AddTransient<MacrosPage>();
        services.AddTransient<CustomizationPage>();
        services.AddTransient<MonitorPage>();
        services.AddTransient<UtilitiesPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<StabilityPage>();
        services.AddTransient<SocialPage>();
    }
}
