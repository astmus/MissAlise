using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUsersRepository
	{
		Task<IUserStorage> GetUserStorageAsync(User user, CancellationToken cancel);
	}
}
