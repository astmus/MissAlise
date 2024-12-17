namespace MissAlise.Background.Settings
{
	public record BackgroundJobNotification
	{
		public virtual string Key { get; set; } = null!;
		public virtual string Description { get; set; } = null!;
		public virtual Guid ServerId { get; set; }
		public virtual Guid EntityId { get; set; }
		public virtual DateTime Occured { get; set; }
		public virtual string Status { get; set; } = null!;
		public virtual string Event { get; set; } = null!;
		public virtual string Scope { get; set; } = null!;
		public virtual string Object { get; set; } = null!;
		public virtual JobCompletionState? Completed { get; set; }
		//public virtual BackgroundJob? OldState { get; set; } = null!;
	}
}
