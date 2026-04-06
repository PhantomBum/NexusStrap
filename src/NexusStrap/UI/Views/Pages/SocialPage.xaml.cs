using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NexusStrap.UI.ViewModels;

namespace NexusStrap.UI.Views.Pages;

public partial class SocialPage : Page
{
    public SocialPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<SocialViewModel>();
    }
}
