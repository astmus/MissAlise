using MissAlise.Utils;

namespace MissAlise.Background
{
	public abstract partial record BackgroundJob
	{		
		public abstract string Key { get; init; }
		public virtual string Description { get; set; } = null!;
		public virtual bool IsEnabled { get; set; } = true;
		public virtual JobCompletionState? State { get; set; }
		public virtual DateTime? LastStart { get; set; }
		public virtual DateTime? LastEnd { get; set; }
		public virtual int Weight { get; set; }
		public virtual int ExecutorsCount { get; set; } = 1;

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
			State = state;
			LastEnd = Time.Now;
		}

		Action? _cancelJob;
		public void SetCancel(Action cancelJob)
			=> _cancelJob = cancelJob;
		public void CancelJob()
			=> _cancelJob?.Invoke();
	}

	public partial record BackgroundJob<TJob> : BackgroundJob where TJob:class
	{
		public static readonly string JobKey = Id<TJob>.UniqueName;
		public TJob Data { get; set; } = null!;
		public override string Key { get; init; } = JobKey;
		public ICollection<EventTrigger<TJob>> Triggers { get; set; } = new List<EventTrigger<TJob>>();

		public static IBackgroundJobBuilder<TJob> CreateBuilder(BackgroundJob<TJob> job)
			=> new Builder(job);		
	}
}