using System.Threading;
using MissAlise.Entities.YandexTracker;

namespace MissAlise.Interfaces
{
	public interface IUsersRepository : IRepository<User>
	{
		Task<(bool Abort, long RowsCopied, DateTime StartTime)> AddOrUpdateUsersAsync(IEnumerable<User> users, CancellationToken cancel);
	}
}
