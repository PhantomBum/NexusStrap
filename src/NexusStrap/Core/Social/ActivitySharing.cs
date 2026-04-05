using NexusStrap.Core.Server;
using NexusStrap.Services;

namespace NexusStrap.Core.Social;

public sealed class ActivitySharing
{
    private readonly ServerBrowser _serverBrowser;
    private readonly LogService _log;

    public ActivitySharing(ServerBrowser serverBrowser, LogService log)
    {
        _serverBrowser = serverBrowser;
        _log = log;
    }

    public string? GenerateJoinLink()
    {
        var server = _serverBrowser.CurrentServer;
        if (server is null) return null;
        return _serverBrowser.GenerateDeepLink(server.GameId, server.ServerId);
    }

    public void CopyJoinLinkToClipboard()
    {
        var link = GenerateJoinLink();
        if (link is not null)
        {
            System.Windows.Clipboard.SetText(link);
            _log.Info("Copied join link to clipboard");
        }
    }
}
