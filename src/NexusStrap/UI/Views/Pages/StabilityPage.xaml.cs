using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class StabilityPage : Page
{
    public StabilityPage()
    {
        DataContext = App.Services.GetRequiredService<StabilityViewModel>();
        InitializeComponent();
    }
}
