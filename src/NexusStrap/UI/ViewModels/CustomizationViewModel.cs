using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.UI.ViewModels;

public partial class CustomizationViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private readonly SettingsService _settings;

    [ObservableProperty] private ObservableCollection<ThemeDefinition> _availableThemes = new();
    [ObservableProperty] private ThemeDefinition _selectedTheme;
    [ObservableProperty] private string _customBackgroundPath = "";
    [ObservableProperty] private double _backgroundOpacity;
    [ObservableProperty] private bool _enableAnimations;

    public CustomizationViewModel(ThemeService themeService, SettingsService settings)
    {
        _themeService = themeService;
        _settings = settings;

        _selectedTheme = themeService.CurrentTheme;
        BackgroundOpacity = settings.Settings.BackgroundOpacity;
        EnableAnimations = settings.Settings.EnableAnimations;
        CustomBackgroundPath = settings.Settings.CustomBackgroundPath ?? "";

        RefreshThemes();
    }

    private void RefreshThemes()
    {
        AvailableThemes = new ObservableCollection<ThemeDefinition>(_themeService.GetAvailableThemes());
    }

    [RelayCommand]
    private void ApplyTheme(ThemeDefinition theme)
    {
        SelectedTheme = theme;
        _themeService.ApplyTheme(theme);
    }

    [RelayCommand]
    private void ApplyDarkTheme() => _themeService.ApplyTheme(AppTheme.Dark);

    [RelayCommand]
    private void ApplyLightTheme() => _themeService.ApplyTheme(AppTheme.Light);

    [RelayCommand]
    private void BrowseBackground()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All Files|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            CustomBackgroundPath = dialog.FileName;
            _settings.Settings.CustomBackgroundPath = dialog.FileName;
            _settings.SaveSettings();
        }
    }

    partial void OnBackgroundOpacityChanged(double value)
    {
        _settings.Settings.BackgroundOpacity = value;
        _settings.SaveSettings();
    }

    partial void OnEnableAnimationsChanged(bool value)
    {
        _settings.Settings.EnableAnimations = value;
        _settings.SaveSettings();
    }
}
