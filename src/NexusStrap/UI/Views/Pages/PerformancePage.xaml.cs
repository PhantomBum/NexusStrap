using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class PerformancePage : Page
{
    public PerformancePage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PerformanceViewModel>();
    }
}
