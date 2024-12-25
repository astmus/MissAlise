#nullable disable
using MissAlise.Utils;

namespace MissAlise.Background
{
	public enum MonthWeek : byte
	{
		First = 1,
		Second = 2,
		Third = 3,
		Fourth = 4
	}

	public record EventTrigger<TJob> : EventTrigger where TJob : class
	{
		public Func<BackgroundJob<TJob>, CancellationToken, ValueTask> FireStarter { get; protected set; }
		private BackgroundJob<TJob> _job;
		protected TJob _jobData { get; set; }
		public override BackgroundJob<TJob> Job => _job;

		public EventTrigger()
		{
			JobKey = Id<TJob>.UniqueName;
		}

		public EventTrigger(TJob jobData = null) : this()
		{
			_jobData = jobData;
		}
		
		public void Setup(BackgroundJob<TJob> job, Func<BackgroundJob<TJob>, CancellationToken, ValueTask> fireStarter)
		{
			_job = job;
			_job.Data = _jobData;
			FireStarter = fireStarter;
		}

		public override ValueTask Fire(CancellationToken cancel)
			=> FireStarter(_job, cancel);
	}

#nullable enable

	public abstract record EventTrigger
	{		
		public string JobKey { get; set; } = null!;
		public string Description { get; set; } = null!;
		public bool IsEnabled { get; set; } = true;
		public abstract BackgroundJob Job { get; }

		public MonthWeek[]? Weeks { get; set; }//переделать на флаг
		public DayOfWeek[]? Days { get; set; }
		public DateOnly? StartAt { get; set; }
		public virtual TimeOnly? RunAt { get; set; }
		public virtual TimeOnly? EndAt { get; set; }
		public virtual TimeSpan? Delay { get; set; }
		public TimeSpan? FreezeTime { get; set; }

		public virtual bool Check()
		{
			if (FreezeTime?.TotalSeconds > 0)
			{
				FreezeTime = FreezeTime?.Subtract(Time.Second);
				return false;
			}

			if (!IsEnabled || !Job.IsEnabled)
				return false;

			var now = Time.Now;
			MonthWeek weekOfMonth = (MonthWeek)((now.Day + (int)now.DayOfWeek) / 7 + 1);

			var isOk =
					   (Weeks is null || Weeks.Contains(weekOfMonth))
				&& (Days is null || Days.Contains(now.DayOfWeek))
				&& (StartAt is null || StartAt.Value <= Time.OnlyDate(now))
				&& (RunAt is null || RunAt.Value < Time.OnlyTime(now))
				&& (EndAt is null || EndAt.Value > Time.OnlyTime(now))
				&& (Job.LastStart is null || now - Job.LastStart.Value >= Delay || Delay is null && now.Day != (Job.LastStart?.Day ?? -1));

			return isOk;
		}

		public abstract ValueTask Fire(CancellationToken cancel);		
	}
#nullable enable
}
