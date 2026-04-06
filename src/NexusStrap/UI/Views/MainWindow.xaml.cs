using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.Services;
using NexusStrap.UI.ViewModels;
using NexusStrap.UI.Views.Pages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NexusStrap.UI.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();

        NavigationView.SetServiceProvider(App.Services);

        var themeService = App.Services.GetRequiredService<ThemeService>();
        themeService.ThemeChanged += _ => Dispatcher.Invoke(ApplyShellBackground);
        ShellBackgroundCoordinator.RefreshRequested += () => Dispatcher.Invoke(ApplyShellBackground);
        ShellBackgroundCoordinator.AppCursorRefreshRequested += () => Dispatcher.Invoke(ApplyAppCursor);

        Loaded += OnLoaded;
        NavigationView.Navigated += OnNavigationViewNavigated;
    }

    private void ApplyShellBackground()
    {
        var settings = App.Services.GetRequiredService<SettingsService>();
        var path = settings.Settings.CustomBackgroundPath;
        var opacity = Math.Clamp(settings.Settings.BackgroundOpacity, 0.02, 1.0);

        var hasImage = !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        if (!hasImage)
        {
            ShellBackgroundImage.Visibility = Visibility.Collapsed;
            ShellBackgroundImage.Source = null;
            ShellTint.Opacity = 1.0;
            return;
        }

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(Path.GetFullPath(path!), UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            ShellBackgroundImage.Source = bmp;
            ShellBackgroundImage.Opacity = opacity;
            ShellBackgroundImage.Visibility = Visibility.Visible;
            ShellTint.Opacity = 0.78;
        }
        catch
        {
            ShellBackgroundImage.Visibility = Visibility.Collapsed;
            ShellTint.Opacity = 1.0;
        }

        ApplyMainWindowBackdrop();
    }

    private void ApplyAppCursor()
    {
        var settings = App.Services.GetRequiredService<SettingsService>();
        if (!settings.Settings.UseCustomAppCursor || string.IsNullOrWhiteSpace(settings.Settings.CustomAppCursorPath))
        {
            Mouse.OverrideCursor = null;
            return;
        }

        var p = settings.Settings.CustomAppCursorPath!;
        if (!File.Exists(p))
        {
            Mouse.OverrideCursor = null;
            return;
        }

        try
        {
            if (p.EndsWith(".cur", StringComparison.OrdinalIgnoreCase) ||
                p.EndsWith(".ani", StringComparison.OrdinalIgnoreCase))
                Mouse.OverrideCursor = new Cursor(p);
            else
                Mouse.OverrideCursor = null;
        }
        catch
        {
            Mouse.OverrideCursor = null;
        }
    }

    /// <summary>
    /// NavigationView hosts content in a DynamicScrollViewer by default, which measures the page with
    /// unbounded height so inner ScrollViewers never get a viewport — wheel and bars do nothing.
    /// The presenter API is internal in WPF-UI; we locate it in the visual tree and set the property via reflection.
    /// </summary>
    private void DisableNavigationOuterScroll()
    {
        NavigationView.ApplyTemplate();
        NavigationView.UpdateLayout();
        var presenter = FindVisualChild<NavigationViewContentPresenter>(NavigationView);
        if (presenter is null) return;

        var prop = typeof(NavigationViewContentPresenter).GetProperty(
            nameof(NavigationViewContentPresenter.IsDynamicScrollViewerEnabled),
            BindingFlags.Public | BindingFlags.Instance);
        var setter = prop?.GetSetMethod(nonPublic: true);
        if (setter is not null)
            setter.Invoke(presenter, new object[] { false });
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent is null) return null;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) return match;
            var nested = FindVisualChild<T>(child);
            if (nested is not null) return nested;
        }

        return null;
    }

    private void OnNavigationViewNavigated(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, DisableNavigationOuterScroll);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        try
        {
            ApplyMainWindowBackdrop();
        }
        catch
        {
            try
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
            }
            catch { /* ignore */ }
        }

        try
        {
            NavigationView.Navigate(typeof(DashboardPage));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Could not open the dashboard.\n\n{ex.Message}",
                "NexusStrap",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, DisableNavigationOuterScroll);

        ApplyShellBackground();
        ApplyAppCursor();
    }

    private void ApplyMainWindowBackdrop()
    {
        var settings = App.Services.GetRequiredService<SettingsService>();
        var themeService = App.Services.GetRequiredService<ThemeService>();
        var hasBg = !string.IsNullOrWhiteSpace(settings.Settings.CustomBackgroundPath)
            && File.Exists(settings.Settings.CustomBackgroundPath!);
        var win11OrLater = Environment.OSVersion.Platform == PlatformID.Win32NT
            && Environment.OSVersion.Version.Build >= 22000;
        var appTheme = themeService.CurrentTheme.IsDark
            ? Wpf.Ui.Appearance.ApplicationTheme.Dark
            : Wpf.Ui.Appearance.ApplicationTheme.Light;

        if (hasBg)
        {
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(appTheme, WindowBackdropType.None);
            return;
        }

        if (win11OrLater)
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(appTheme, WindowBackdropType.Mica);
        else
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(appTheme);
    }
}
