using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        DataContext = App.Services.GetRequiredService<DashboardViewModel>();
        InitializeComponent();
    }
}
