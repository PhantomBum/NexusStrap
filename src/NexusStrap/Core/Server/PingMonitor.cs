using System.Net.NetworkInformation;
using NexusStrap.Services;

namespace NexusStrap.Core.Server;

public sealed class PingMonitor
{
    private readonly LogService _log;

    public PingMonitor(LogService log)
    {
        _log = log;
    }

    public async Task<long> PingAsync(string host, int timeoutMs = 3000)
    {
        try
        {
            using var pingSender = new Ping();
            var reply = await pingSender.SendPingAsync(host, timeoutMs);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
        }
        catch (Exception ex)
        {
            _log.Debug("Ping to {Host} failed: {Msg}", host, ex.Message);
            return -1;
        }
    }

    public async Task<Dictionary<string, long>> PingMultipleAsync(IEnumerable<string> hosts, int timeoutMs = 3000)
    {
        var results = new Dictionary<string, long>();
        var tasks = hosts.Select(async host =>
        {
            var latency = await PingAsync(host, timeoutMs);
            lock (results) results[host] = latency;
        });
        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<string?> FindLowestPingAsync(IEnumerable<string> hosts, int timeoutMs = 3000)
    {
        var results = await PingMultipleAsync(hosts, timeoutMs);
        return results
            .Where(kvp => kvp.Value >= 0)
            .OrderBy(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();
    }
}
