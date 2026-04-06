using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class CustomizationPage : Page
{
    public CustomizationPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<CustomizationViewModel>();
    }
}
