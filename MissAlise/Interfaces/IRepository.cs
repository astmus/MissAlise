using System.Linq.Expressions;

namespace MissAlise.Interfaces
{
	public interface IRepository<T> where T : class
	{		
		Task<IEnumerable<T>> GetAllAsync(CancellationToken cancel);
		//Task AddAsync(T entity, CancellationToken cancel);
		//Task UpdateAsync(T entity, CancellationToken cancel);
		Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel);
	}
}
