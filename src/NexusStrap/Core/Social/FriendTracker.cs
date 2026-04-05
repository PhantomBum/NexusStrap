using NexusStrap.Services;

namespace NexusStrap.Core.Social;

public sealed class FriendTracker
{
    private readonly LogService _log;
    private readonly List<FriendActivity> _activities = new();

    public IReadOnlyList<FriendActivity> Activities => _activities;

    public FriendTracker(LogService log)
    {
        _log = log;
    }

    public void AddActivity(FriendActivity activity)
    {
        _activities.Insert(0, activity);
        if (_activities.Count > 100)
            _activities.RemoveAt(_activities.Count - 1);
    }

    public void Clear() => _activities.Clear();
}

public sealed class FriendActivity
{
    public string Username { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string? GameId { get; set; }
    public DateTime Timestamp { get; set; }
    public FriendActivityType Type { get; set; }
}

public enum FriendActivityType { Joined, Left, ServerSwitch }
