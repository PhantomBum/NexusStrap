using System.Collections.Concurrent;

namespace NexusStrap.Services;

public sealed class EventBus
{
    private readonly ConcurrentDictionary<string, List<Action<object?>>> _handlers = new();

    public void Subscribe(string eventName, Action<object?> handler)
    {
        var list = _handlers.GetOrAdd(eventName, _ => new List<Action<object?>>());
        lock (list)
        {
            list.Add(handler);
        }
    }

    public void Unsubscribe(string eventName, Action<object?> handler)
    {
        if (_handlers.TryGetValue(eventName, out var list))
        {
            lock (list)
            {
                list.Remove(handler);
            }
        }
    }

    public void Publish(string eventName, object? data = null)
    {
        if (!_handlers.TryGetValue(eventName, out var list)) return;

        Action<object?>[] snapshot;
        lock (list)
        {
            snapshot = list.ToArray();
        }

        foreach (var handler in snapshot)
        {
            try
            {
                handler(data);
            }
            catch
            {
                // swallow per-handler exceptions to avoid breaking event propagation
            }
        }
    }
}
