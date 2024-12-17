#nullable disable
using MissAlise.Utils;

namespace MissAlise.Background
{
	public abstract record BackgroundJob
	{
		public virtual string Key { get; protected set; }
		public virtual string Description { get; set; }
		public virtual JobCompletionState? Completed { get; set; }
		public virtual DateTime? LastStart { get; set; }
		public virtual DateTime? LastEnd { get; set; }
		public virtual int Weight { get; set; }
		public virtual int ExecutorsCount { get; set; } = 1;

		//public IEntityServer Organization { get; init; }

		JobState _jobState = 0;
		public JobState GetState()
			=> _jobState;
		public void PendingState()
			=> _jobState = JobState.Pending;
		public void ResetState()
			=> _jobState = JobState.ReadyToRun;
		public void Start()
		{
			_jobState = JobState.Executing;
			LastStart = Time.Now.TrimSeconds();
		}
		public void End(JobCompletionState state)
		{
			Completed = state;
			LastEnd = Time.Now;
		}

		Action _cancelJob;
		public void SetCancel(Action cancelJob)
			=> _cancelJob = cancelJob;
		public void CancelJob()
			=> _cancelJob?.Invoke();

		public record Builder
		{
			private readonly BackgroundJob _job;
			public Builder(BackgroundJob job)
				=> _job = job;
			public Builder SetDescription(string description)
			{
				_job.Description = description;
				return this;
			}
			public Builder SetWeight(int weight)
			{
				_job.Weight = weight;
				return this;
			}
			public Builder SetExecutors(int count)
			{
				_job.ExecutorsCount = count;
				return this;
			}
		}
	}

	public record BackgroundJob<TJob> : BackgroundJob
	{
		public static readonly string JobKey = typeof(TJob).FullName;
		public TJob JobData { get; set; }
		public override string Key { get; protected set; } = JobKey;
	}
}
#nullable enable