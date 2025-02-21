namespace MissAlise.Background
{
	public abstract record EventTrigger
	{
		public string JobKey { get; set; } = null!;
		public string Description { get; set; } = null!;
		public bool IsEnabled { get; set; } = true;
		public abstract IBackgroundJob Job { get; }
		public MonthWeek[]? Weeks { get; set; }//переделать на флаг
		public DayOfWeek[]? Days { get; set; }
		public DateOnly? StartAt { get; set; }
		public virtual TimeOnly? RunAt { get; set; }
		public virtual TimeOnly? EndAt { get; set; }
		public virtual TimeSpan? Delay { get; set; }
		public TimeSpan? FreezeTime { get; set; }
		public abstract bool Check();	
		public abstract ValueTask Fire(CancellationToken cancel);
	}
}
