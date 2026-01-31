namespace MissAlise.Interfaces;

using MissAlise.Entities.Media;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public interface IMediaRepository
{
    Task<IReadOnlyDictionary<(ProviderKind Provider, string RemoteItemId, string? DriveId), MediaItemId>> ResolveIdsBySourceAsync(
        IReadOnlyCollection<(ProviderKind Provider, string RemoteItemId, string? DriveId)> sources,
        CancellationToken ct = default);

    Task UpsertRangeAsync(
        IReadOnlyCollection<MediaItem> items,
        CancellationToken ct = default);

    Task<IReadOnlyList<MediaItem>> GetByKindAsync(
        MediaKind kind,
        int skip,
        int take,
        CancellationToken ct = default);

    Task<MediaItem?> GetByIdAsync(MediaItemId id, CancellationToken ct = default);
}
