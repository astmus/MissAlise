using MissAlise.Utils;

namespace MissAlise.Background
{		
	public partial record BackgroundJob<TJob> : BackgroundJob where TJob : class
	{		
		public TJob Data { get; set; } = null!;
		public override string Key { get; init; } = Identity<TJob>.Discriminator;
		public ICollection<EventTrigger<TJob>> Triggers { get; set; } = new List<EventTrigger<TJob>>();	

		JobState jobState = 0;
		public JobState GetState()
			=> jobState;
		public void PendingState()
			=> jobState = JobState.Pending;
		public void ResetState()
			=> jobState = JobState.ReadyToRun;
		public void StartJob()
		{
			jobState = JobState.Executing;
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