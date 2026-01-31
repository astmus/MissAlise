namespace MissAlise.Entities.Media;

using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public readonly record struct MediaLibraryId(Guid Value)
{
    public static MediaLibraryId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
	public static implicit operator Guid(MediaLibraryId id) => id.Value;

}

public sealed class MediaLibrary
{
    public MediaLibraryId Id { get; }
    public UserId OwnerId { get; }
    public ProviderKind Provider { get; }
    public SyncState Sync { get; private set; }

    public MediaLibrary(MediaLibraryId id, UserId ownerId, ProviderKind provider, SyncState? sync = null)
    {
        Id = id;
        OwnerId = ownerId;
        Provider = provider;
        Sync = sync ?? SyncState.Empty;
    }

    public void MarkSyncStarted(DateTimeOffset now)
        => Sync = Sync.Start(now);

    public void MarkSyncSuccess(DateTimeOffset now, string? newCursor)
        => Sync = Sync.Success(now, newCursor);

    public void MarkSyncFailed(DateTimeOffset now, Exception ex)
        => Sync = Sync.Fail(now, ex.Message);
}
