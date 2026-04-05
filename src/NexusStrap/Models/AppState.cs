namespace NexusStrap.Models;

public sealed class AppState
{
    public DateTime LastLaunch { get; set; }
    public string? LastGameId { get; set; }
    public string? LastServerId { get; set; }
    public List<SessionRecord> RecentSessions { get; set; } = new();
    public string? LastActiveAccount { get; set; }
    public Dictionary<string, string> AccountCookies { get; set; } = new();
}

public sealed class SessionRecord
{
    public string GameId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string? ServerId { get; set; }
    public string? ServerRegion { get; set; }
    public DateTime JoinTime { get; set; }
    public TimeSpan Duration { get; set; }
}
