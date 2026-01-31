namespace MissAlise.Interfaces;

using MissAlise.Entities.Media;
using MissAlise.ValueObjects.Storage;

public interface IMediaLibraryRepository
{
    Task<MediaLibrary> GetOrCreateAsync(Guid ownerUserId, ProviderKind provider, CancellationToken ct = default);
    Task SaveAsync(MediaLibrary library, CancellationToken ct = default);
}
