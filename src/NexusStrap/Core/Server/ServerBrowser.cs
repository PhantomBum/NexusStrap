using System.Text.Json;
using System.Text.RegularExpressions;
using NexusStrap.Core.Bootstrapper;
using NexusStrap.Models;
using NexusStrap.Services;

namespace NexusStrap.Core.Server;

public sealed partial class ServerBrowser
{
    private readonly PingMonitor _pingMonitor;
    private readonly HttpService _http;
    private readonly LogService _log;

    public ServerInfo? CurrentServer { get; private set; }
    public event Action<ServerInfo>? ServerChanged;

    public ServerBrowser(PingMonitor pingMonitor, HttpService http, LogService log)
    {
        _pingMonitor = pingMonitor;
        _http = http;
        _log = log;
    }

    public ServerInfo? ParseServerFromLog(string logContent)
    {
        try
        {
            // Parse server IP and port from Roblox log
            var udpMatch = UdpConnectRegex().Match(logContent);
            if (!udpMatch.Success) return null;

            var ip = udpMatch.Groups["ip"].Value;
            var port = int.Parse(udpMatch.Groups["port"].Value);

            // Parse server instance ID
            var instanceMatch = InstanceIdRegex().Match(logContent);
            var serverId = instanceMatch.Success ? instanceMatch.Groups[1].Value : "unknown";

            // Parse place ID
            var placeMatch = PlaceIdRegex().Match(logContent);
            var gameId = placeMatch.Success ? placeMatch.Groups[1].Value : "unknown";

            var server = new ServerInfo
            {
                ServerId = serverId,
                GameId = gameId,
                IpAddress = ip,
                Port = port,
                JoinTime = DateTime.Now
            };

            CurrentServer = server;
            ServerChanged?.Invoke(server);
            return server;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse server info from log");
            return null;
        }
    }

    public async Task<ServerInfo?> MonitorLatestLogAsync(CancellationToken ct = default)
    {
        var logsDir = RegistryManager.GetRobloxLogsPath();
        if (!Directory.Exists(logsDir)) return null;

        var latestLog = Directory.GetFiles(logsDir, "*.log")
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();

        if (latestLog is null) return null;

        var content = await File.ReadAllTextAsync(latestLog, ct);
        return ParseServerFromLog(content);
    }

    public async Task<string> DetectRegionAsync(string ip)
    {
        try
        {
            var response = await _http.GetStringAsync($"http://ip-api.com/json/{ip}?fields=country,regionName,city");
            var doc = JsonDocument.Parse(response);
            var country = doc.RootElement.GetProperty("country").GetString() ?? "Unknown";
            var city = doc.RootElement.GetProperty("city").GetString() ?? "";
            return string.IsNullOrEmpty(city) ? country : $"{city}, {country}";
        }
        catch
        {
            return "Unknown";
        }
    }

    public string GenerateDeepLink(string placeId, string? serverInstanceId = null)
    {
        if (serverInstanceId is not null)
            return $"roblox://experiences/start?placeId={placeId}&gameInstanceId={serverInstanceId}";
        return $"roblox://experiences/start?placeId={placeId}";
    }

    [GeneratedRegex(@"UDMUX client  connected to (?<ip>[\d.]+)\|(?<port>\d+)")]
    private static partial Regex UdpConnectRegex();

    [GeneratedRegex(@"gameId:\s*(\S+)")]
    private static partial Regex InstanceIdRegex();

    [GeneratedRegex(@"placeId:\s*(\d+)")]
    private static partial Regex PlaceIdRegex();
}
