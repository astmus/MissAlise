using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUserRepository
	{
		Task<IUserStorage> GetUserStorageAsync(User user, CancellationToken cancel);
	}
}
