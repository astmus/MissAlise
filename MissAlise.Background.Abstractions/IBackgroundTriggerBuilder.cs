namespace MissAlise.Background
{
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