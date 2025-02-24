using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	internal class UserProfilesRepository : IUserProfilesRepository
	{
		private readonly IMongoCollection<DbUserProfile> userProfiles;
		private readonly IdentityContext identity;
		private readonly ILogger<UserProfilesRepository> logger;
		static ReplaceOptions options = new ReplaceOptions { IsUpsert = true };

		public UserProfilesRepository(IMongoDatabase database, IdentityContext identity, ILogger<UserProfilesRepository> logger)
		{
			userProfiles = database.GetCollection<DbUserProfile>("UserProfiles");
			this.identity = identity;
			this.logger = logger;
		}


		public async Task<UserProfile> FindAsync(string id, CancellationToken cancel)
		{
			var filter = Builders<DbUserProfile>.Filter.Eq(r => r.Telegram.Id, id);
			var projection = Builders<DbUserProfile>.Projection.Exclude("_id");
			return await userProfiles.Find(filter)/*.Project<DbUserProfile>(projection)*/.FirstOrDefaultAsync(cancel);
		}

		public async Task AddOrReplaceAsync(UserProfile user, CancellationToken cancel)
		{
			try
			{
				var filter = Builders<DbUserProfile>.Filter.Eq(r => r.Telegram.Id, user.Telegram.Id);
				var projection = Builders<UserProfile>.Projection.Combine();
				var profile = await userProfiles.Find(filter)/*.Project<DbUserProfile>(projection)*/.FirstOrDefaultAsync(cancel);
				profile ??= new()
				{
					AccessData = user.AccessData,
					Telegram = user.Telegram
				};
				var update = Builders<DbUserProfile>.Update.Set("AccessData", user.AccessData);
				var res = await userProfiles.UpdateOneAsync(filter, update);
			}
			catch (Exception error)
			{
				logger.LogError(error, error.Message);
			}
		}

		public async Task<IEnumerable<UserProfile>> AllAsync(CancellationToken cancel)
		{
			var profiles = await userProfiles.Find(_ => true).ToListAsync();
			return profiles;
		}

		public async Task AddPendingUser(User user, CancellationToken cancel)
		{
			await identity.PendingUsers.AddAsync(user, cancel).ConfigureAwait(false);
			await identity.SaveChangesAsync(cancel);
		}

		public async Task<User?> PopPendingUser(string identifier, CancellationToken cancel)
		{
			var user = await identity.PendingUsers.FirstOrDefaultAsync(user => user.Id == identifier, cancel).ConfigureAwait(false);
			if (user != null)
			{
				identity.PendingUsers.Remove(user);
				await identity.SaveChangesAsync(cancel);
			}

			return user;
		}
	}

}
