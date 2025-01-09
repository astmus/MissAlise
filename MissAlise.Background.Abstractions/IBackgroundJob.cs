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
}
