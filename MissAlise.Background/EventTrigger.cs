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
		public Func<BackgroundJob<TJob>, CancellationToken, ValueTask> FireStarter { get; init; }
		BackgroundJob<TJob> _job;

		public override BackgroundJob Job => _job;

		public EventTrigger()
			=> JobKey = Id<TJob>.Name;

		public void SetJob(BackgroundJob<TJob> job)
		{
			_job = job;
		}

		public override ValueTask Fire(CancellationToken cancel)
			=> FireStarter(_job, cancel);
	}

#nullable enable

	public abstract record EventTrigger
	{
		public string JobKey { get; set; } = null!;
		public string Description { get; set; } = null!;
		public MonthWeek[]? Weeks { get; set; }//переделать на флаг
		public DayOfWeek[]? Days { get; set; }
		public bool IsEnabled { get; set; } = true;
		public DateOnly? StartAt { get; set; }
		public virtual TimeOnly? RunAt { get; set; }
		public virtual TimeOnly? EndAt { get; set; }
		public virtual TimeSpan? Delay { get; set; }
		public TimeSpan? FreezeTime { get; set; }

		public bool Check()
		{
			if (FreezeTime?.TotalSeconds > 0)
			{
				FreezeTime = FreezeTime?.Subtract(Time.Second);
				return false;
			}

			if (!IsEnabled)
				return false;

			var now = Time.Now;
			MonthWeek weekOfMonth = (MonthWeek)((now.Day + (int)now.DayOfWeek) / 7 + 1);

			var isOk = 
					   (Weeks is null	|| Weeks.Contains(weekOfMonth))
				&& (Days is null		|| Days.Contains(now.DayOfWeek))
				&& (StartAt is null	|| StartAt.Value <= Time.OnlyDate(now))
				&& (RunAt is null	|| RunAt.Value < Time.OnlyTime(now))
				&& (EndAt is null		|| EndAt.Value > Time.OnlyTime(now))
				&& (Job.LastStart is null || now - Job.LastStart.Value >= Delay || Delay is null && now.Day != (Job.LastStart?.Day ?? -1))
			;

			return isOk;
		}

		public abstract BackgroundJob Job { get; }
		public abstract ValueTask Fire(CancellationToken cancel);

		public class Builder
		{
			private readonly EventTrigger _trigger;
			public Builder(EventTrigger trigger)
				=> _trigger = trigger;

			public Builder SetDescription(in string description)
			{
				_trigger.Description = description;
				return this;
			}

			public Builder SetEnabled(in bool isEnabled = true)
			{
				_trigger.IsEnabled = isEnabled;
				return this;
			}

			public Builder SetFreezTime(in TimeSpan freezeTime)
			{
				_trigger.FreezeTime = freezeTime;
				return this;
			}
			public Builder SetWeeks(params MonthWeek[] weeks)
			{
				_trigger.Weeks = weeks;
				return this;
			}
			public Builder SetDays(params DayOfWeek[] days)
			{
				_trigger.Days = days;
				return this;
			}
			public Builder SetDelay(in TimeSpan delay)
			{
				_trigger.Delay = delay;
				return this;
			}
			public Builder SetStartAt(in DateOnly startAt)
			{
				_trigger.StartAt = startAt;
				return this;
			}
			public Builder SetRunAt(in TimeOnly runAt)
			{
				_trigger.RunAt = runAt;
				return this;
			}
			public Builder SetEndAt(in TimeOnly endAt)
			{
				_trigger.EndAt = endAt;
				return this;
			}
		}
	}

#nullable enable
}
