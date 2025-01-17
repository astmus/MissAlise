using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUserProfilesRepository
	{
		Task AddOrReplaceAsync(UserProfile user, CancellationToken cancel);
		Task<UserProfile> FindAsync(string id, CancellationToken cancel);
	}
}
