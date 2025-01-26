using LinqToDB.EntityFrameworkCore;
using MissAlise.DataBase.Models;
using MissAlise.Entities.OneDrive;

namespace MissAlise.DataBase
{
	internal abstract class ItemsRepository
	{
		protected UserMediaContext ctx;

		public Task<TItem?> GetItemById<TItem>(int itemId, CancellationToken cancel) where TItem : Item
			=> ctx.Set<TItem>().FirstOrDefaultAsyncEF(item => item.Itemid == itemId, cancel);
	}
}
