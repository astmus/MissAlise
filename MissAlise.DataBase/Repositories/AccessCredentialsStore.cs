using MissAlise.Entities.Identity;
using MissAlise.Interfaces;
using MissAlise.DataBase.Models;
using MissAlise.ValueObjects;
using MongoDB.Driver;

namespace MissAlise.DataBase.Repositories
{
	internal sealed class AccessCredentialsStore : IAccessCredentialsStore
	{
		private readonly IMongoCollection<DbAccessCredentials> _collection;
		private static readonly ReplaceOptions Upsert = new() { IsUpsert = true };

		public AccessCredentialsStore(IMongoDatabase database)
		{
			_collection = database.GetCollection<DbAccessCredentials>("AccessCredentials");
		}

		public async Task<AccessInformation?> GetAsync(UserId userId, CancellationToken ct = default)
		{
			var db = await _collection
				.Find(x => x.OwnerId == userId.Value)
				.FirstOrDefaultAsync(ct);
			return db is null ? null : ToDomain(db);
		}

		public async Task SaveAsync(UserId userId, AccessInformation access, CancellationToken ct = default)
		{
			var db = ToDb(userId, access);
			var filter = Builders<DbAccessCredentials>.Filter.Eq(x => x.OwnerId, userId.Value);
			await _collection.ReplaceOneAsync(filter, db, Upsert, ct);
		}

		private static AccessInformation ToDomain(DbAccessCredentials db) => new()
		{
			TokenType = db.TokenType,
			Scope = db.Scope,
			ExpiresIn = db.ExpiresIn,
			AccessToken = db.AccessToken,
			RefreshToken = db.RefreshToken,
			IdToken = db.IdToken,
			ExpiredAfter = db.ExpiredAfter
		};

		private static DbAccessCredentials ToDb(UserId userId, AccessInformation access) => new()
		{
			OwnerId = userId.Value,
			TokenType = access.TokenType ?? string.Empty,
			Scope = access.Scope ?? string.Empty,
			ExpiresIn = access.ExpiresIn,
			AccessToken = access.AccessToken ?? string.Empty,
			RefreshToken = access.RefreshToken ?? string.Empty,
			IdToken = access.IdToken ?? string.Empty,
			ExpiredAfter = access.ExpiredAfter,
			UpdatedAt = DateTimeOffset.UtcNow
		};
	}
}
