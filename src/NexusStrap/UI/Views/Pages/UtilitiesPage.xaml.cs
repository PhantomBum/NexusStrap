using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class UtilitiesPage : Page
{
    public UtilitiesPage()
    {
        DataContext = App.Services.GetRequiredService<UtilitiesViewModel>();
        InitializeComponent();
    }
}
