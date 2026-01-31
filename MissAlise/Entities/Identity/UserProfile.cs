using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Identity;

namespace MissAlise.Entities.Identity
{
	public sealed class UserProfile
	{
		public UserId UserId { get; private set; }

		private List<ExternalIdentity> identities = new();
		public IReadOnlyCollection<ExternalIdentity> Identities => identities;

		public DateTimeOffset CreatedAt { get; private set; }

		private UserProfile() { } // для Mongo

		public UserProfile(Guid ownerId, IEnumerable<ExternalIdentity>? identities = null)
		{
			UserId = new UserId(ownerId);
			CreatedAt = DateTimeOffset.UtcNow;

			if (identities != null)
				foreach (var id in identities)
					AddIdentity(id);
		}

		public bool HasIdentity(ExternalIdentity identity)
			=> identities.Any(x => x.Equals(identity));

		public void AddIdentity(ExternalIdentity identity)
		{
			if (HasIdentity(identity))
				return;

			identities.Add(identity);
		}
	}
}
