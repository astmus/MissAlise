namespace MissAlise.DataBase.Repositories;

using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Models;
using MissAlise.Entities.Media;
using MissAlise.Interfaces;
using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public sealed class MediaLibraryRepository : IMediaLibraryRepository
{
    private readonly UserMediaContext _db;

    public MediaLibraryRepository(UserMediaContext db) => _db = db;

    public async Task<MediaLibrary> GetOrCreateAsync(Guid ownerUserId, ProviderKind provider, CancellationToken ct = default)
    {
        var row = await _db.MediaLibraries
            .FirstOrDefaultAsync(x => x.OwnerId == ownerUserId && x.Provider == provider, ct);

        if (row is null)
        {
            row = new DbMediaLibrary
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerUserId,
                Provider = provider,
                DeltaToken = null,
                LastFullSyncAt = null,
                LastDeltaSyncAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            _db.MediaLibraries.Add(row);
            await _db.SaveChangesAsync(ct);
        }

        return new MediaLibrary(
            id: new MediaLibraryId(row.Id),
            ownerId: new UserId(row.OwnerId),
            provider: row.Provider,
			sync: new SyncState(
				Cursor: row.DeltaToken,
				LastStartedAt: row.LastSyncStartedAt,
				LastFinishedAt: row.LastSyncFinishedAt,
				Status: (SyncStatus)row.LastSyncStatus,
				LastError: row.LastSyncError
			)
		);
    }

	public async Task SaveAsync(MediaLibrary library, CancellationToken ct = default)
	{
		var row = await _db.MediaLibraries
			.FirstOrDefaultAsync(x => x.Id == library.Id, ct);

		if (row is null)
		{
			row = new DbMediaLibrary
			{
				Id = library.Id,
				OwnerId = library.OwnerId.Value,
				Provider = library.Provider,
				CreatedAt = DateTimeOffset.UtcNow
			};

			_db.MediaLibraries.Add(row);
		}

		// SyncState -> db columns
		row.DeltaToken = library.Sync.Cursor;
		row.LastSyncStartedAt = library.Sync.LastStartedAt;
		row.LastSyncFinishedAt = library.Sync.LastFinishedAt;
		row.LastSyncStatus = library.Sync.Status;
		row.LastSyncError = library.Sync.LastError;

		row.UpdatedAt = DateTimeOffset.UtcNow;

		await _db.SaveChangesAsync(ct);
	}
}
