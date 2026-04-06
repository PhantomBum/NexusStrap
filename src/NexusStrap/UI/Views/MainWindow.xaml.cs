using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;
using NexusStrap.UI.Views.Pages;
using Wpf.Ui.Controls;

namespace NexusStrap.UI.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();

        NavigationView.SetServiceProvider(App.Services);

        Loaded += OnLoaded;
        NavigationView.Navigated += OnNavigationViewNavigated;
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
            // Mica requires Windows 11 (build 22000+). On Windows 10, Mica can fail silently or break the window.
            var win11OrLater = Environment.OSVersion.Platform == PlatformID.Win32NT
                && Environment.OSVersion.Version.Build >= 22000;

            if (win11OrLater)
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Dark,
                    WindowBackdropType.Mica);
            }
            else
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
            }
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
    }
}
