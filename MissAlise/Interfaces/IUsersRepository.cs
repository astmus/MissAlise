using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUsersRepository
	{
		Task AddOrReplaceAsync(UserProfile user, CancellationToken cancel);
		Task<UserProfile> FindAsync(string name, CancellationToken cancel);
	}
}
