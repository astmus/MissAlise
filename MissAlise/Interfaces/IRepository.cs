using System.Linq.Expressions;
using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IRepository<T> : IDisposable where T : class
	{
		Task<IItemsPage<T>> GetPageAsync(int page, int perPage, CancellationToken cancel);
		Task<bool> IsExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel);
		Task<T> AddAsync(T item, CancellationToken cancel);
		Task<T?> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel);
		Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel);
		Task<TItem?> GetItemById<TItem>(int itemId, CancellationToken cancel) where TItem : ItemInfo;
	}
}
