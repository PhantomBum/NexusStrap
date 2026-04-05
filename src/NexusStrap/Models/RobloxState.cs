namespace NexusStrap.Models;

public sealed class RobloxState
{
    public string? InstalledVersionGuid { get; set; }
    public string? InstalledVersionNumber { get; set; }
    public string? InstallDirectory { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? Channel { get; set; } = "LIVE";
    public List<string> InstalledVersions { get; set; } = new();
}
