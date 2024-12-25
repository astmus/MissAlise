using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace MissAlise.Interfaces
{

	public interface IRepository
	{
		IQueryable<TResult> RawQuery<TResult>(string rawQuery = null, params object[] args);
		Task<TEntity[]> RawQueryAsync<TEntity>(string rawQuery, CancellationToken cancel = default) where TEntity : class;
	}

	public interface IRepository<in TData> : IRepository where TData : class
	{
		IQueryable<TItem> Query<TItem>() where TItem : TData;
		IEnumerable<TItem> GetAll<TItem>() where TItem : TData;
		Task<TItem?> FindAsync<TItem>(Expression<Func<TItem, bool>> predicate, CancellationToken cancel = default) where TItem : class, TData;
		Task<IEnumerable<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancel = default) where TEntity : class, TData;
		Task<IEnumerable<TEntity>> GetPageAsync<TEntity>(ushort page, ushort perPage, Expression<Func<TEntity, bool>> predicate, CancellationToken cancel = default) where TEntity : class, TData;
		Task<IEnumerable<TItem>> GetAllAsync<TItem>(CancellationToken cancel) where TItem : TData;
		int Update<TItem>(TItem item) where TItem : TData;
		Task<int> UpdateAsync<TEntity>(TEntity item, CancellationToken cancel) where TEntity : TData;
		int InsertOrUpdate<TItem>(TItem item) where TItem : TData;
		Task<int> InsertOrUpdateAsync<TItem>(TItem item, CancellationToken cancel) where TItem : TData;
		Task<(bool Abort, long RowsCopied, DateTime StartTime)> AddOrUpdateAsync<TItem>(IEnumerable<TItem> items, CancellationToken cancel) where TItem : class, TData;
	}
}
