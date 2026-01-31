using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Models;
using MissAlise.Entities.Identity;
using MissAlise.Entities.Media;
using MissAlise.Interfaces;
using MissAlise.ValueObjects.Identity;
using MongoDB.Driver;

namespace MissAlise.DataBase.Repositories
{
	internal class UserProfilesRepository : IUserProfilesRepository
	{
		private readonly IMongoCollection<DbUserProfile> profiles;		
		private readonly ILogger<UserProfilesRepository> _log;
		static ReplaceOptions options = new ReplaceOptions { IsUpsert = true };
		private const string CollectionName = "UserProfiles";		

		public UserProfilesRepository(IMongoDatabase database, ILogger<UserProfilesRepository> logger)
		{
			profiles = database.GetCollection<DbUserProfile>("UserProfiles");			
			_log = logger;
		}

		public async Task<UserProfile?> FindByOwnerIdAsync(Guid ownerId, CancellationToken ct)
		{
			var db = await profiles
				.Find(x => x.OwnerId == ownerId)
				.FirstOrDefaultAsync(ct);

			return db is null ? null : ToDomain(db);
		}

		public async Task<UserProfile?> FindByExternalIdentityAsync(ExternalIdentity identity, CancellationToken ct)
		{
			var filter = ByIdentity(identity);

			var db = await profiles
				.Find(filter)
				.FirstOrDefaultAsync(ct);

			return db is null ? null : ToDomain(db);
		}

		public async Task SaveAsync(UserProfile profile, CancellationToken ct)
		{
			var db = ToDb(profile);

			db.UpdatedAt = DateTimeOffset.UtcNow;

			// Upsert по OwnerId (это твой внутренний PK)
			var filter = Builders<DbUserProfile>.Filter.Eq(x => x.OwnerId, profile.UserId.Value);
			var options = new ReplaceOptions { IsUpsert = true };

			await profiles.ReplaceOneAsync(filter, db, options, ct);			
		}

		// helpers
		private static FilterDefinition<DbUserProfile> ByIdentity(ExternalIdentity identity)
		{
			var scheme = Normalize(identity.Scheme);

			return Builders<DbUserProfile>.Filter.ElemMatch(
				x => x.Identities,
				i => i.Scheme == scheme && i.ExternalId == identity.ExternalId);
		}

		private static string Normalize(string scheme) => scheme.Trim().ToLowerInvariant();

		private static UserProfile ToDomain(DbUserProfile db)
		{
			var ids = db.Identities
				.Select(i => new ExternalIdentity(i.Scheme, i.ExternalId));

			// если у тебя конструктор другой — подстрой
			var profile = new UserProfile(db.OwnerId, ids);			
			return profile;
		}

		private static DbUserProfile ToDb(UserProfile domain)
		{
			return new DbUserProfile
			{
				OwnerId = domain.UserId.Value,
				Identities = domain.Identities
					.Select(i => new DbExternalIdentity
					{
						Scheme = Normalize(i.Scheme),
						ExternalId = i.ExternalId,
						IsPrimary = false,          // можно вычислять, если нужно
						LinkedAt = DateTimeOffset.UtcNow
					})
					.ToList()
			};
		}
	}
}
