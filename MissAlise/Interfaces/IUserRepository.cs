using MissAlise.Entities.Media;
using MissAlise.ValueObjects;

namespace MissAlise.Interfaces
{
	public interface IUserRepository
	{
		Task<IUserStorage> GetStorageAsync(UserId user, CancellationToken cancel);
	}
}
