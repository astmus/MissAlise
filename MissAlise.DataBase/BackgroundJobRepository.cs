using MissAlise.Background;
using MissAlise.Utils;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	internal class BackgroundJobRepository : IBackgroundJobRepository
	{
		private readonly IMongoCollection<BackgroundJob> _backgroundJobs;
		
		public BackgroundJobRepository(IMongoDatabase database)
		{
			_backgroundJobs = database.GetCollection<BackgroundJob>("BackgroundJobs");		
		}

		public async Task<TJob> LoadAsync<TJob>(string jobKey, CancellationToken cancel) where TJob : BackgroundJob
		{
			var _filter = Builders<BackgroundJob>.Filter.Eq(r => r.Key, jobKey);
			var projection = Builders<BackgroundJob>.Projection.Exclude("_id");
			return await _backgroundJobs.Find(_filter).Project<TJob>(projection).FirstOrDefaultAsync(cancel);			
		}

		static ReplaceOptions options = new ReplaceOptions { IsUpsert = true };
		public async Task AddOrReplaceAsync<TJob>(TJob backgroundJob, CancellationToken cancel) where TJob : BackgroundJob
		{
			var _filter = Builders<BackgroundJob>.Filter.Eq(r => r.Key, backgroundJob.Key);
			var res = await _backgroundJobs.ReplaceOneAsync(_filter, backgroundJob, options, cancel);			
		}
	}
}
