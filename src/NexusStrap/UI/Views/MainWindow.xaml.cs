using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;
using NexusStrap.UI.Views.Pages;
using Wpf.Ui.Controls;

namespace NexusStrap.UI.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow()
    {
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        InitializeComponent();

        NavigationView.SetServiceProvider(App.Services);

        Loaded += (_, _) =>
        {
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark, WindowBackdropType.Mica);
            NavigationView.Navigate(typeof(DashboardPage));
        };
    }
}
