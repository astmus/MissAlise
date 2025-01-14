using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	internal class UsersRepository : IUsersRepository
	{
		private readonly IMongoCollection<UserProfile> userProfiles;
		
		public UsersRepository(IMongoDatabase database)
		{
			userProfiles = database.GetCollection<UserProfile>("Users");		
		}

		static ReplaceOptions options = new ReplaceOptions { IsUpsert = true };

		public async Task<UserProfile> FindAsync(string name, CancellationToken cancel)
		{
			var _filter = Builders<UserProfile>.Filter.Eq(r => r.Id, name);	
			return await userProfiles.Find(_filter).FirstOrDefaultAsync(cancel);			
		}
		public async Task AddOrReplaceAsync(UserProfile user, CancellationToken cancel)
		{
			var _filter = Builders<UserProfile>.Filter.Eq(r => r.Id, user.Id);
			var res = await userProfiles.ReplaceOneAsync(_filter, user, options, cancel);		
		}
	}
}
