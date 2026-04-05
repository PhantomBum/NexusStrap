namespace NexusStrap.Models;

public sealed class ServerInfo
{
    public string ServerId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public int Port { get; set; }
    public string Region { get; set; } = "Unknown";
    public double PingMs { get; set; }
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public DateTime JoinTime { get; set; }
    public TimeSpan Uptime { get; set; }
}
