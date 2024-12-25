namespace MissAlise.Background
{
	public interface IBackgroundJobRepository
	{
		Task AddOrReplaceAsync<TJob>(TJob backgroundJob, CancellationToken cancel) where TJob : BackgroundJob;
		Task<TJob> LoadAsync<TJob>(string jobKey, CancellationToken cancel) where TJob : BackgroundJob;
	}
}
