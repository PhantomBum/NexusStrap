using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class MacrosPage : Page
{
    public MacrosPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MacrosViewModel>();
    }
}
