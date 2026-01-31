namespace MissAlise.DataBase.Repositories;

using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Mapping;
using MissAlise.DataBase.Models;
using MissAlise.Entities.Media;
using MissAlise.Interfaces;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public sealed partial class MediaRepository : IMediaRepository
{
    private readonly UserMediaContext _db;

    public MediaRepository(UserMediaContext db) => _db = db;

    public async Task<IReadOnlyDictionary<(ProviderKind Provider, string RemoteItemId, string? DriveId), MediaItemId>> ResolveIdsBySourceAsync(
        IReadOnlyCollection<(ProviderKind Provider, string RemoteItemId, string? DriveId)> sources,
        CancellationToken ct = default)
    {
        if (sources.Count == 0) return new Dictionary<(ProviderKind, string, string?), MediaItemId>();

        var providers = sources.Select(s => s.Provider).Distinct().ToArray();
        var remoteIds = sources.Select(s => s.RemoteItemId).Distinct().ToArray();

        var candidates = await _db.MediaItems
            .AsNoTracking()
            .Where(x => providers.Contains(x.Provider) && remoteIds.Contains(x.RemoteItemId))
            .Select(x => new { x.Id, x.Provider, x.RemoteItemId, x.DriveId })
            .ToListAsync(ct);

        var wanted = new HashSet<(ProviderKind, string, string?)>(sources);

        var result = new Dictionary<(ProviderKind, string, string?), MediaItemId>(sources.Count);
        foreach (var c in candidates)
        {
            var k = ((ProviderKind)c.Provider, c.RemoteItemId, c.DriveId);
            if (!wanted.Contains(k)) continue;
            result[k] = new MediaItemId(c.Id.ToString());
        }

        return result;
    }

    public async Task UpsertRangeAsync(IReadOnlyCollection<MediaItem> items, CancellationToken ct = default)
    {
        if (items.Count == 0) return;

        var keys = items.Select(i => (i.Provider, i.RemoteItemId, i.DriveId)).ToArray();
        var providers = keys.Select(k => k.Provider).Distinct().ToArray();
        var remoteIds = keys.Select(k => k.RemoteItemId).Distinct().ToArray();

        // Fetch existing rows that *might* match; then match precisely in-memory (including DriveId).
        var existing = await _db.MediaItems
            .Where(x => providers.Contains(x.Provider) && remoteIds.Contains(x.RemoteItemId))
            .ToListAsync(ct);

        var dict = existing.ToDictionary(x => (x.Provider, x.RemoteItemId, x.DriveId));

        foreach (var item in items)
        {
            var k = (item.Provider, item.RemoteItemId, item.DriveId);

            if (dict.TryGetValue(k, out var dbItem))
            {
                MediaDbMapper.ApplyToExisting(dbItem, item);
            }
            else
            {
                var added = MediaDbMapper.ToDb(item);
                await _db.MediaItems.AddAsync(added, ct);
                dict[k] = added;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MediaItem>> GetByKindAsync(MediaKind kind, int skip, int take, CancellationToken ct = default)
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 50;

        var rows = await _db.MediaItems
            .AsNoTracking()
            .Where(x => x.Kind == kind && !x.IsDeleted)
            .OrderByDescending(x => x.TakenAt ?? x.ModifiedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return rows.Select(MediaDbMapper.ToDomain).ToList();
    }

    public async Task<MediaItem?> GetByIdAsync(MediaItemId id, CancellationToken ct = default)
    {
        var row = await _db.MediaItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.Value, ct);
        return row is null ? null : MediaDbMapper.ToDomain(row);
    }
}
