using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Bulk;
using MissAlise.DataBase.Contexts;
using MissAlise.Entities.Media;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

namespace MissAlise.DataBase.Repositories
{
	internal sealed class MediaSyncRepository
	{
		private readonly UserMediaContext ctx;

		public MediaSyncRepository(UserMediaContext ctx)
		{
			this.ctx = ctx;
		}

		// 1️⃣ ResolveIdsBySource (для identityResolver)
		public async Task<Dictionary<(ProviderKind Provider, string RemoteItemId, string? DriveId), string>> ResolveIdsAsync(
			Guid ownerId,
			IReadOnlyCollection<(ProviderKind Provider, string RemoteItemId, string? DriveId)> sources,
			CancellationToken ct)
		{
			if (sources.Count == 0)
				return new();

			var remoteIds = sources.Select(s => s.RemoteItemId).Distinct().ToArray();
			var providers = sources.Select(s => s.Provider).Distinct().ToArray();
			var driveIds = sources.Select(s => s.DriveId ?? "").Distinct().ToArray();

			var candidates = await ctx.MediaItems.AsNoTracking()
				.Where(x =>
					x.OwnerId == ownerId &&
					remoteIds.Contains(x.RemoteItemId) &&
					providers.Contains(x.Provider) &&
					driveIds.Contains(x.DriveId ?? ""))
				.Select(x => new
				{
					x.Id,
					x.Provider,
					x.RemoteItemId,
					x.DriveId
				})
				.ToListAsync(ct);

			var set = sources.ToHashSet();

			return candidates
				.Select(x => (Key: ((ProviderKind)x.Provider, x.RemoteItemId, x.DriveId), x.Id))
				.Where(x => set.Contains(x.Key))
				.ToDictionary(x => x.Key, x => x.Id);
		}

		public Task BulkUpsertAsync(IReadOnlyCollection<MediaItem> items, CancellationToken ct)
			=> PostgresCopyMerge.BulkUpsertViaCopyAsync(ctx, items, ct);
	}

}
