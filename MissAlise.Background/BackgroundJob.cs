using MissAlise.Utils;

namespace MissAlise.Background
{	
	
	public partial record BackgroundJob<TJob> : BackgroundJob, IBackgroundJob<TJob> where TJob : class
	{		
		public TJob Data { get; set; } = null!;
		public override string Key { get; init; } = Id<TJob>.UniqueName;
		public ICollection<EventTrigger<TJob>> Triggers { get; set; } = new List<EventTrigger<TJob>>();

		public static IBackgroundJobBuilder<TJob> CreateBuilder(BackgroundJob<TJob> job)
			=> new Builder(job);

		JobState _jobState = 0;
		public JobState GetState()
			=> _jobState;
		public void PendingState()
			=> _jobState = JobState.Pending;
		public void ResetState()
			=> _jobState = JobState.ReadyToRun;
		public void StartJob()
		{
			_jobState = JobState.Executing;
			LastStart = Time.Now.TrimSeconds();
		}
		public void EndJob(JobCompletionState state)
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
}