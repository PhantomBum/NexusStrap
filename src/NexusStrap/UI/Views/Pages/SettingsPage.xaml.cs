using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        DataContext = App.Services.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
    }
}
