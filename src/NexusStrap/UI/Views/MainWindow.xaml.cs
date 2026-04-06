using System.Windows;
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
    }
}
