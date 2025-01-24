using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	internal class UserProfilesRepository : IUserProfilesRepository
	{
		private readonly IMongoCollection<UserProfile> userProfiles;
		
		public UserProfilesRepository(IMongoDatabase database)
		{
			userProfiles = database.GetCollection<UserProfile>("UserProfiles");		
		}

		static ReplaceOptions options = new ReplaceOptions { IsUpsert = true };

		public async Task<UserProfile> FindAsync(string id, CancellationToken cancel)
		{
			var _filter = Builders<UserProfile>.Filter.Eq(r => r.Id, id);	
			return await userProfiles.Find(_filter).FirstOrDefaultAsync(cancel);			
		}

		public async Task AddOrReplaceAsync(UserProfile user, CancellationToken cancel)
		{
			var _filter = Builders<UserProfile>.Filter.Eq(r => r.Id, user.Id);
			var res = await userProfiles.ReplaceOneAsync(_filter, user, options, cancel);		
		}

		public async Task<IEnumerable<UserProfile>> AllAsync(CancellationToken cancel)
		{
			var profiles = await userProfiles.Find(_ => true).ToListAsync();
			return profiles;
		}
	}
}
