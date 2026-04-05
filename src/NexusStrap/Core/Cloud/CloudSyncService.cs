namespace NexusStrap.Core.Cloud;

public interface ICloudSyncService
{
    Task<bool> SyncSettingsAsync(CancellationToken ct = default);
    Task<bool> SyncThemesAsync(CancellationToken ct = default);
    Task<bool> SyncPresetsAsync(CancellationToken ct = default);
    bool IsAvailable { get; }
}

public sealed class LocalCloudSyncStub : ICloudSyncService
{
    public bool IsAvailable => false;

    public Task<bool> SyncSettingsAsync(CancellationToken ct = default) => Task.FromResult(false);
    public Task<bool> SyncThemesAsync(CancellationToken ct = default) => Task.FromResult(false);
    public Task<bool> SyncPresetsAsync(CancellationToken ct = default) => Task.FromResult(false);
}
