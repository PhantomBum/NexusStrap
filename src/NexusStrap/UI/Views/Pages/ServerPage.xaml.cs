using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class ServerPage : Page
{
    public ServerPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ServerViewModel>();
    }
}
