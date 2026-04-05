using System.Text.Json;
using System.Text.Json.Serialization;
using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

public sealed class VersionChecker
{
    private const string VersionApiUrl = "https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer";
    private const string ChannelApiTemplate = "https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer/channel/{0}";

    private readonly HttpService _http;
    private readonly LogService _log;

    public VersionChecker(HttpService http, LogService log)
    {
        _http = http;
        _log = log;
    }

    public async Task<RobloxVersionInfo?> GetLatestVersionAsync(string channel = "LIVE", CancellationToken ct = default)
    {
        try
        {
            var url = channel == "LIVE"
                ? VersionApiUrl
                : string.Format(ChannelApiTemplate, channel);

            _log.Info("Checking latest Roblox version from {Url}", url);
            var info = await _http.GetJsonAsync<RobloxVersionInfo>(url, ct);
            if (info is not null)
            {
                _log.Info("Latest version: {VersionGuid} ({Version})", info.ClientVersionUpload, info.Version);
            }
            return info;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to check Roblox version");
            return null;
        }
    }

    public bool IsUpdateRequired(string? currentVersionGuid, string latestVersionGuid)
    {
        return !string.Equals(currentVersionGuid, latestVersionGuid, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class RobloxVersionInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("clientVersionUpload")]
    public string ClientVersionUpload { get; set; } = string.Empty;

    [JsonPropertyName("bootstrapperVersion")]
    public string BootstrapperVersion { get; set; } = string.Empty;
}
