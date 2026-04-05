namespace NexusStrap.Models;

public sealed class PluginManifest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string EntryDll { get; set; } = string.Empty;
    public string? MinHostVersion { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? IconPath { get; set; }
    public string? Homepage { get; set; }
}
