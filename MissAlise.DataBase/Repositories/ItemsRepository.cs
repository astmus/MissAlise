using LinqToDB.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.Entities.OneDrive;

namespace MissAlise.DataBase
{
	internal abstract class ItemsRepository : IDisposable
	{		
		protected UserMediaContext ctx;

		public Task<TItem?> GetItemById<TItem>(int itemId, CancellationToken cancel) where TItem : ItemInfo
			=> ctx.Set<TItem>().FirstOrDefaultAsyncEF(item => item.Id == itemId, cancel);
		
		public void Dispose() 
			=> ctx.SaveChanges();
	}
}
