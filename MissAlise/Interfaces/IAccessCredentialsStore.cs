using MissAlise.Entities.Identity;
using MissAlise.ValueObjects;

namespace MissAlise.Interfaces
{
	/// <summary>
	/// Хранилище OAuth-креденшелов (AccessInformation) в MongoDB, отдельно от UserProfile.
	/// </summary>
	public interface IAccessCredentialsStore
	{
		Task<AccessInformation?> GetAsync(UserId userId, CancellationToken ct = default);
		Task SaveAsync(UserId userId, AccessInformation access, CancellationToken ct = default);
	}
}
