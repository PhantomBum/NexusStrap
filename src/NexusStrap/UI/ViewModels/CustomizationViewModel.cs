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

    [ObservableProperty] private bool _useCustomAppCursor;
    [ObservableProperty] private string _customAppCursorPath = "";

    [ObservableProperty] private bool _enableCustomRobloxCursor;
    [ObservableProperty] private string _customRobloxCursorPath = "";

    public CustomizationViewModel(ThemeService themeService, SettingsService settings)
    {
        _themeService = themeService;
        _settings = settings;

        _selectedTheme = themeService.CurrentTheme;
        BackgroundOpacity = settings.Settings.BackgroundOpacity;
        EnableAnimations = settings.Settings.EnableAnimations;
        CustomBackgroundPath = settings.Settings.CustomBackgroundPath ?? "";

        UseCustomAppCursor = settings.Settings.UseCustomAppCursor;
        CustomAppCursorPath = settings.Settings.CustomAppCursorPath ?? "";
        EnableCustomRobloxCursor = settings.Settings.EnableCustomRobloxCursor;
        CustomRobloxCursorPath = settings.Settings.CustomRobloxCursorPath ?? "";

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
            ShellBackgroundCoordinator.RequestRefresh();
        }
    }

    [RelayCommand]
    private void ClearBackground()
    {
        CustomBackgroundPath = "";
        _settings.Settings.CustomBackgroundPath = null;
        _settings.SaveSettings();
        ShellBackgroundCoordinator.RequestRefresh();
    }

    [RelayCommand]
    private void BrowseAppCursor()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Cursor|*.cur;*.ani|All Files|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            CustomAppCursorPath = dialog.FileName;
            _settings.Settings.CustomAppCursorPath = dialog.FileName;
            _settings.SaveSettings();
            ShellBackgroundCoordinator.RequestAppCursorRefresh();
        }
    }

    [RelayCommand]
    private void ClearAppCursor()
    {
        CustomAppCursorPath = "";
        _settings.Settings.CustomAppCursorPath = null;
        _settings.Settings.UseCustomAppCursor = false;
        UseCustomAppCursor = false;
        _settings.SaveSettings();
        ShellBackgroundCoordinator.RequestAppCursorRefresh();
    }

    [RelayCommand]
    private void BrowseRobloxCursor()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PNG image|*.png|All Files|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            CustomRobloxCursorPath = dialog.FileName;
            _settings.Settings.CustomRobloxCursorPath = dialog.FileName;
            _settings.SaveSettings();
        }
    }

    [RelayCommand]
    private void ClearRobloxCursor()
    {
        CustomRobloxCursorPath = "";
        _settings.Settings.CustomRobloxCursorPath = null;
        _settings.Settings.EnableCustomRobloxCursor = false;
        EnableCustomRobloxCursor = false;
        _settings.SaveSettings();
    }

    partial void OnBackgroundOpacityChanged(double value)
    {
        _settings.Settings.BackgroundOpacity = value;
        _settings.SaveSettings();
        ShellBackgroundCoordinator.RequestRefresh();
    }

    partial void OnEnableAnimationsChanged(bool value)
    {
        _settings.Settings.EnableAnimations = value;
        _settings.SaveSettings();
    }

    partial void OnUseCustomAppCursorChanged(bool value)
    {
        _settings.Settings.UseCustomAppCursor = value;
        _settings.SaveSettings();
        ShellBackgroundCoordinator.RequestAppCursorRefresh();
    }

    partial void OnEnableCustomRobloxCursorChanged(bool value)
    {
        _settings.Settings.EnableCustomRobloxCursor = value;
        _settings.SaveSettings();
    }
}
