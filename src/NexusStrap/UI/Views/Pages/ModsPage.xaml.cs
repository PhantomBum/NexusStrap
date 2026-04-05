using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class ModsPage : Page
{
    public ModsPage()
    {
        DataContext = App.Services.GetRequiredService<ModsViewModel>();
        InitializeComponent();
    }
}
