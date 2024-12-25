namespace MissAlise.Background
{
	public interface IBackgroundJobBuilder<TJob>
	{
		IBackgroundJobBuilder<TJob> SetDescription(string description);
		IBackgroundJobBuilder<TJob> SetExecutors(int count);
		IBackgroundJobBuilder<TJob> SetWeight(int weight);
		IBackgroundJobBuilder<TJob> IsDisabled(int count);
		IBackgroundTriggerBuilder<TJob> AddTrigger(TJob jobData, string description);
	}

	public interface IBackgroundTriggerBuilder<TJob>
	{
		IBackgroundTriggerBuilder<TJob> SetDescription(in string description);
		IBackgroundTriggerBuilder<TJob> SetEnabled(in bool isEnabled = true);
		IBackgroundTriggerBuilder<TJob> SetFreezTime(in TimeSpan freezeTime);
		IBackgroundTriggerBuilder<TJob> SetWeeks(params MonthWeek[] weeks);
		IBackgroundTriggerBuilder<TJob> SetDays(params DayOfWeek[] days);
		IBackgroundTriggerBuilder<TJob> SetDelay(in TimeSpan delay);
		IBackgroundTriggerBuilder<TJob> SetStartAt(in DateOnly startAt);
		IBackgroundTriggerBuilder<TJob> SetRunAt(in TimeOnly runAt);
		IBackgroundTriggerBuilder<TJob> SetEndAt(in TimeOnly endAt);
	}
}