using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace NexusStrap.Services;

public sealed class HttpService : IDisposable
{
    private readonly HttpClient _client;

    private static readonly string[] CdnUrls =
    {
        "https://setup.rbxcdn.com/",
        "https://setup-ak.rbxcdn.com/",
        "https://roblox-setup.cachefly.net/",
        "https://s3.amazonaws.com/setup.roblox.com/"
    };

    public HttpService()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("NexusStrap/1.0");
        _client.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<T?> GetJsonAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await _client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    public async Task<string> GetStringAsync(string url, CancellationToken ct = default)
    {
        return await _client.GetStringAsync(url, ct);
    }

    public async Task<byte[]> GetBytesAsync(string url, CancellationToken ct = default)
    {
        return await _client.GetByteArrayAsync(url, ct);
    }

    public async Task DownloadFileAsync(string url, string destinationPath,
        IProgress<double>? progress = null, CancellationToken ct = default)
    {
        using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var dir = Path.GetDirectoryName(destinationPath);
        if (dir is not null) Directory.CreateDirectory(dir);

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[81920];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            totalRead += bytesRead;
            if (totalBytes > 0)
                progress?.Report((double)totalRead / totalBytes);
        }
    }

    public async Task<string?> DownloadFromCdnAsync(string relativePath, string destinationPath,
        IProgress<double>? progress = null, CancellationToken ct = default)
    {
        foreach (var cdn in CdnUrls)
        {
            try
            {
                var url = cdn + relativePath;
                await DownloadFileAsync(url, destinationPath, progress, ct);
                return cdn;
            }
            catch
            {
                continue;
            }
        }
        return null;
    }

    public void Dispose() => _client.Dispose();
}
