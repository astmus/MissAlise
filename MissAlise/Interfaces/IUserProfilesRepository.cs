using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUserProfilesRepository
	{
		Task<IEnumerable<UserProfile>> AllAsync(CancellationToken cancel);
		Task AddOrReplaceAsync(UserProfile user, CancellationToken cancel);
		Task<UserProfile> FindAsync(string id, CancellationToken cancel);
	}
}
