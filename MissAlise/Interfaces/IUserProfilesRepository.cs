using MissAlise.Entities.Identity;
using MissAlise.Entities.Media;
using MissAlise.ValueObjects.Identity;

namespace MissAlise.Interfaces
{
	public interface IUserProfilesRepository
	{
		Task<UserProfile?> FindByOwnerIdAsync(Guid ownerId, CancellationToken ct);
		Task<UserProfile?> FindByExternalIdentityAsync(ExternalIdentity identity, CancellationToken ct);
		Task SaveAsync(UserProfile profile, CancellationToken ct);
	}
}
