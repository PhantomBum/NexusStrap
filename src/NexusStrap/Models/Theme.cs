namespace NexusStrap.Models;

public sealed class ThemeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public bool IsDark { get; set; } = true;
    public string PrimaryColor { get; set; } = "#7C3AED";
    public string SecondaryColor { get; set; } = "#A855F7";
    public string BackgroundColor { get; set; } = "#0F0F0F";
    public string SurfaceColor { get; set; } = "#1A1A2E";
    public string TextColor { get; set; } = "#FFFFFF";
    public string AccentColor { get; set; } = "#7C3AED";
    public double BlurRadius { get; set; } = 20;
    public double Opacity { get; set; } = 0.85;
    public string? BackgroundImagePath { get; set; }
    public string? FontFamily { get; set; }
    public string? CursorPath { get; set; }
}
