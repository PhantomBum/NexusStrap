using Microsoft.Win32;
using NexusStrap.Services;

namespace NexusStrap.Core.Bootstrapper;

public sealed class ProtocolHandler
{
    private readonly LogService _log;

    public ProtocolHandler(LogService log)
    {
        _log = log;
    }

    public void RegisterProtocols(string exePath)
    {
        RegisterProtocol("roblox-player", exePath);
        RegisterProtocol("roblox", exePath);
        _log.Info("Registered protocol handlers for NexusStrap at {Path}", exePath);
    }

    public void UnregisterProtocols()
    {
        UnregisterProtocol("roblox-player");
        UnregisterProtocol("roblox");
        _log.Info("Unregistered NexusStrap protocol handlers");
    }

    public static bool IsRegistered()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\roblox-player\shell\open\command");
            var value = key?.GetValue(null)?.ToString() ?? string.Empty;
            return value.Contains("NexusStrap", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static LaunchArgs ParseProtocolUri(string uri)
    {
        var args = new LaunchArgs();

        if (string.IsNullOrWhiteSpace(uri))
            return args;

        // roblox://experiences/start?placeId=123&gameInstanceId=456
        if (uri.StartsWith("roblox://", StringComparison.OrdinalIgnoreCase))
        {
            args.Protocol = "roblox";
            args.RawUri = uri;
            var uriObj = new Uri(uri);
            var query = System.Web.HttpUtility.ParseQueryString(uriObj.Query);
            args.PlaceId = query["placeId"];
            args.GameInstanceId = query["gameInstanceId"];
            args.LaunchData = query["launchData"];
            args.AccessCode = query["accessCode"];
            args.LinkCode = query["linkCode"];
            return args;
        }

        // roblox-player:1+launchmode:play+robloxLocale:en_us+...
        if (uri.StartsWith("roblox-player:", StringComparison.OrdinalIgnoreCase))
        {
            args.Protocol = "roblox-player";
            args.RawUri = uri;
            var pairs = uri.Split('+');
            foreach (var pair in pairs)
            {
                var parts = pair.Split(':', 2);
                if (parts.Length == 2)
                {
                    args.Parameters[parts[0].Trim()] = parts[1].Trim();
                }
            }
            args.PlaceId = args.Parameters.GetValueOrDefault("placeId");
            args.GameInstanceId = args.Parameters.GetValueOrDefault("gameInstanceId");
            return args;
        }

        return args;
    }

    private void RegisterProtocol(string protocol, string exePath)
    {
        try
        {
            var keyPath = $@"SOFTWARE\Classes\{protocol}";
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            key.SetValue(null, $"URL: {protocol} Protocol");
            key.SetValue("URL Protocol", string.Empty);

            using var commandKey = Registry.CurrentUser.CreateSubKey($@"{keyPath}\shell\open\command");
            commandKey.SetValue(null, $"\"{exePath}\" \"%1\"");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to register protocol {Protocol}", protocol);
        }
    }

    private void UnregisterProtocol(string protocol)
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree($@"SOFTWARE\Classes\{protocol}", false);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to unregister protocol {Protocol}", protocol);
        }
    }
}

public sealed class LaunchArgs
{
    public string Protocol { get; set; } = string.Empty;
    public string RawUri { get; set; } = string.Empty;
    public string? PlaceId { get; set; }
    public string? GameInstanceId { get; set; }
    public string? LaunchData { get; set; }
    public string? AccessCode { get; set; }
    public string? LinkCode { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}
