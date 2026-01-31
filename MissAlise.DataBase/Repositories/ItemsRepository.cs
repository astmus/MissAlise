using LinqToDB.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.Entities.Media;

namespace MissAlise.DataBase.Repositories
{
	internal abstract class ItemsRepository : IDisposable
	{
		protected UserMediaContext ctx;

		public Task<TItem?> GetById<TItem>(string itemId, CancellationToken cancel) where TItem : MediaItem
			=> ctx.Set<TItem>().FirstOrDefaultAsyncEF(item => item.Id == itemId, cancel);

		public void Dispose()
			=> ctx.SaveChanges();
	}
}
