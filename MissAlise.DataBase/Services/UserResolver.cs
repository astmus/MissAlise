namespace MissAlise.DataBase.Services;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MissAlise.Interfaces;
using MissAlise.ValueObjects.Identity;
using MissAlise.DataBase.Models;
using MissAlise.ValueObjects;

public sealed class MongoOwnerResolver : IOwnerResolver
{
	private readonly IMongoCollection<DbUserProfile> userProfiles;

	public MongoOwnerResolver(IMongoDatabase database)
	{
		userProfiles = database.GetCollection<DbUserProfile>("UserProfiles");
	}

	public async Task<Guid> ResolveOwnerIdAsync(ExternalIdentity identity, CancellationToken ct)
	{
		var filter = Builders<DbUserProfile>.Filter.ElemMatch(
			x => x.Identities,
			i => i.Scheme == identity.Scheme && i.ExternalId == identity.ExternalId);

		var profile = await userProfiles.Find(filter).FirstOrDefaultAsync(ct);

		if (profile is not null)
			return profile.OwnerId;

		// Создаём новый owner
		var userId = Guid.NewGuid();

		var newProfile = new DbUserProfile
		{
			OwnerId = userId,
			Identities = new List<DbExternalIdentity>
			{
				new DbExternalIdentity
				{
					Scheme = identity.Scheme,
					ExternalId = identity.ExternalId,
					IsPrimary = true
				}
			}
		};

		try
		{
			await userProfiles.InsertOneAsync(newProfile, cancellationToken: ct);
			return userId;
		}
		catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
		{
			// Гонка: кто-то параллельно создал профиль с той же identity
			var existing = await userProfiles.Find(filter).FirstAsync(ct);
			return existing.OwnerId;
		}
	}
}