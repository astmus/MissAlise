namespace MissAlise.Background
{
	public interface IBackgroundJobBuilder<TJob>
	{
		IBackgroundJobBuilder<TJob> SetDescription(string description);
		IBackgroundJobBuilder<TJob> SetExecutors(int count);
		IBackgroundJobBuilder<TJob> SetWeight(int weight);
		IBackgroundJobBuilder<TJob> IsDisabled();
		IBackgroundTriggerBuilder<TJob> AddTrigger(TJob jobData, string description);
	}
}