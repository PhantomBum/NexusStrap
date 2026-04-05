using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using NexusStrap.Models;

namespace NexusStrap.Services;

public sealed class ThemeService
{
    private readonly SettingsService _settings;
    private ThemeDefinition _currentTheme;

    public ThemeDefinition CurrentTheme => _currentTheme;

    public event Action<ThemeDefinition>? ThemeChanged;

    public ThemeService(SettingsService settings)
    {
        _settings = settings;
        _currentTheme = GetDefaultDarkTheme();
    }

    public void ApplyTheme(AppTheme theme)
    {
        _currentTheme = theme switch
        {
            AppTheme.Light => GetDefaultLightTheme(),
            AppTheme.Custom when _settings.Settings.CustomThemePath is not null =>
                LoadCustomTheme(_settings.Settings.CustomThemePath) ?? GetDefaultDarkTheme(),
            _ => GetDefaultDarkTheme()
        };

        ApplyToResources(_currentTheme);
        ThemeChanged?.Invoke(_currentTheme);
    }

    public void ApplyTheme(ThemeDefinition theme)
    {
        _currentTheme = theme;
        ApplyToResources(theme);
        ThemeChanged?.Invoke(theme);
    }

    public IReadOnlyList<ThemeDefinition> GetAvailableThemes()
    {
        var themes = new List<ThemeDefinition> { GetDefaultDarkTheme(), GetDefaultLightTheme() };

        var themesDir = _settings.ThemesDirectory;
        if (Directory.Exists(themesDir))
        {
            foreach (var file in Directory.GetFiles(themesDir, "*.json"))
            {
                var custom = LoadCustomTheme(file);
                if (custom is not null) themes.Add(custom);
            }
        }

        return themes;
    }

    private static ThemeDefinition? LoadCustomTheme(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ThemeDefinition>(json);
        }
        catch { return null; }
    }

    private void ApplyToResources(ThemeDefinition theme)
    {
        var app = Application.Current;
        if (app is null) return;

        app.Dispatcher.Invoke(() =>
        {
            app.Resources["ThemePrimaryBrush"] = new SolidColorBrush(ParseColor(theme.PrimaryColor));
            app.Resources["ThemeSecondaryBrush"] = new SolidColorBrush(ParseColor(theme.SecondaryColor));
            app.Resources["ThemeBackgroundBrush"] = new SolidColorBrush(ParseColor(theme.BackgroundColor));
            app.Resources["ThemeSurfaceBrush"] = new SolidColorBrush(ParseColor(theme.SurfaceColor));
            app.Resources["ThemeTextBrush"] = new SolidColorBrush(ParseColor(theme.TextColor));
            app.Resources["ThemeAccentBrush"] = new SolidColorBrush(ParseColor(theme.AccentColor));

            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                theme.IsDark ? Wpf.Ui.Appearance.ApplicationTheme.Dark : Wpf.Ui.Appearance.ApplicationTheme.Light);
        });
    }

    private static Color ParseColor(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return Colors.White; }
    }

    private static ThemeDefinition GetDefaultDarkTheme() => new()
    {
        Id = "dark", Name = "NexusStrap Dark", Author = "NexusStrap", IsDark = true,
        PrimaryColor = "#7C3AED", SecondaryColor = "#A855F7",
        BackgroundColor = "#0F0F0F", SurfaceColor = "#1A1A2E",
        TextColor = "#FFFFFF", AccentColor = "#7C3AED"
    };

    private static ThemeDefinition GetDefaultLightTheme() => new()
    {
        Id = "light", Name = "NexusStrap Light", Author = "NexusStrap", IsDark = false,
        PrimaryColor = "#7C3AED", SecondaryColor = "#A855F7",
        BackgroundColor = "#F8F8F8", SurfaceColor = "#FFFFFF",
        TextColor = "#1A1A2E", AccentColor = "#7C3AED"
    };
}
