namespace MissAlise.Entities.YandexTracker
{
	public record Organization : Entity
	{
		//Timestamp CreatedAt { get; set; } not needed yet		
		public virtual string Name { get; set; }
		public virtual string? Title { get; set; }
		public virtual string? Description { get; set; }
		public virtual string OrganizationId { get; set; }
		public virtual string ServiceAccountToken { get; set; }
		public virtual string SecurityKey { get; set; }
		public virtual bool IsEnabled { get; set; }
		//MapField<string, string> Labels { get; } not needed yet
	}
}