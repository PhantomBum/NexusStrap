namespace NexusStrap.Models;

public sealed class ModInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public bool IsEnabled { get; set; }
    public string DirectoryPath { get; set; } = string.Empty;
    public List<ModFileMapping> FileMappings { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public sealed class ModFileMapping
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
}
