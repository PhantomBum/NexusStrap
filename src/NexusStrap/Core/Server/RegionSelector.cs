using NexusStrap.Services;

namespace NexusStrap.Core.Server;

public sealed class RegionSelector
{
    private readonly PingMonitor _pingMonitor;
    private readonly LogService _log;

    public static readonly IReadOnlyDictionary<string, string> KnownRegions = new Dictionary<string, string>
    {
        ["US-East"] = "us-east-1",
        ["US-West"] = "us-west-2",
        ["US-Central"] = "us-central-1",
        ["EU-West"] = "eu-west-1",
        ["EU-Central"] = "eu-central-1",
        ["Asia-Pacific (Singapore)"] = "ap-southeast-1",
        ["Asia-Pacific (Japan)"] = "ap-northeast-1",
        ["South America"] = "sa-east-1",
        ["Australia"] = "ap-southeast-2"
    };

    public string SelectedRegion { get; set; } = "Auto";
    public event Action<string>? RegionChanged;

    public RegionSelector(PingMonitor pingMonitor, LogService log)
    {
        _pingMonitor = pingMonitor;
        _log = log;
    }

    public void SetRegion(string region)
    {
        SelectedRegion = region;
        RegionChanged?.Invoke(region);
        _log.Info("Region set to: {Region}", region);
    }
}
