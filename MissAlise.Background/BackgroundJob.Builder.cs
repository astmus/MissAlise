#nullable disable
namespace MissAlise.Background
{
	public partial record BackgroundJob<TJob>
	{
		internal record Builder : IBackgroundJobBuilder<TJob>, IBackgroundTriggerBuilder<TJob>
		{
			public readonly BackgroundJob<TJob> _job;
			public Builder(BackgroundJob<TJob> job)
				=> _job = job;

			public IBackgroundJobBuilder<TJob> SetDescription(string description)
			{
				_job.Description = description;
				return this;
			}

			public IBackgroundJobBuilder<TJob> SetWeight(int weight)
			{
				_job.Weight = weight;
				return this;
			}
			public IBackgroundJobBuilder<TJob> SetExecutors(int count)
			{
				_job.ExecutorsCount = count;
				return this;
			}

			public IBackgroundJobBuilder<TJob> IsDisabled()
			{
				_job.IsEnabled = false;
				return this;
			}

			private EventTrigger<TJob> _trigger;
			public IBackgroundTriggerBuilder<TJob> AddTrigger(TJob jobData, string description)
			{
				_trigger = new EventTrigger<TJob>(jobData) { Description = description };
				_job.Triggers.Add(_trigger);
				return this;
			}
			
			public IBackgroundTriggerBuilder<TJob> SetDescription(in string description)
			{				
				_trigger.Description = description;
				return this;
			}

			public IBackgroundTriggerBuilder<TJob> SetEnabled(in bool isEnabled = true)
			{
				_trigger.IsEnabled = isEnabled;
				return this;
			}

			public IBackgroundTriggerBuilder<TJob> SetFreezTime(in TimeSpan freezeTime)
			{
				_trigger.FreezeTime = freezeTime;
				return this;
			}
			public IBackgroundTriggerBuilder<TJob> SetWeeks(params MonthWeek[] weeks)
			{
				_trigger.Weeks = weeks;
				return this;
			}
			public IBackgroundTriggerBuilder<TJob> SetDays(params DayOfWeek[] days)
			{
				_trigger.Days = days;
				return this;
			}
			public IBackgroundTriggerBuilder<TJob> SetDelay(in TimeSpan delay)
			{
				_trigger.Delay = delay;
				return this;
			}
			public IBackgroundTriggerBuilder<TJob> SetStartAt(in DateOnly startAt)
			{
				_trigger.StartAt = startAt;
				return this;
			}
			public IBackgroundTriggerBuilder<TJob> SetRunAt(in TimeOnly runAt)
			{
				_trigger.RunAt = runAt;
				return this;
			}
			public IBackgroundTriggerBuilder<TJob> SetEndAt(in TimeOnly endAt)
			{
				_trigger.EndAt = endAt;
				return this;
			}
		}
	}
}
#nullable enable