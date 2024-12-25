namespace MissAlise.Background
{
	public interface IBackgroundJob
	{
		string Key { get; init; }
		string Description { get; set; }
		int ExecutorsCount { get; set; }
		bool IsEnabled { get; set; }
		DateTime? LastEnd { get; set; }
		DateTime? LastStart { get; set; }
		JobCompletionState? State { get; set; }
		int Weight { get; set; }
	}

	public interface IBackgroundJob<TJob> : IBackgroundJob where TJob : class
	{
		TJob Data { get; set; }
		//ICollection<EventTrigger<TJob>> Triggers { get; set; }
		JobState GetState();
		void PendingState();
		void ResetState();
		void StartJob();
		void EndJob(JobCompletionState state);
		void CancelJob();
		void SetCancel(Action cancelJob);
	}
}
