namespace MissAlise.Background
{
	public abstract record BackgroundJob : IBackgroundJob
	{
		public abstract string Key { get; init; }
		public virtual string Description { get; set; } = null!;
		public virtual bool IsEnabled { get; set; } = true;
		public virtual JobCompletionState? State { get; set; }
		public virtual DateTime? LastStart { get; set; }
		public virtual DateTime? LastEnd { get; set; }
		public virtual int Weight { get; set; }
		public virtual int ExecutorsCount { get; set; } = 1;
	}
}
