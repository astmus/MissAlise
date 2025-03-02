#nullable disable
using MissAlise.Utils;

namespace MissAlise.Background
{
	public record EventTrigger<TJob> : EventTrigger where TJob : class
	{
		public Func<BackgroundJob<TJob>, CancellationToken, ValueTask> FireStarter { get; protected set; }
		private BackgroundJob<TJob> _job;
		protected TJob _jobData { get; set; }
		public override BackgroundJob<TJob> Job => _job;

		public EventTrigger()
		{
			JobKey = Identity<TJob>.Discriminator;
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

		public override bool Check()
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
	}

#nullable restore
}
