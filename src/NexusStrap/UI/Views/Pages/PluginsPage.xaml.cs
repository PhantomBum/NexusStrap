using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class PluginsPage : Page
{
    public PluginsPage()
    {
        DataContext = App.Services.GetRequiredService<PluginsViewModel>();
        InitializeComponent();
    }
}
