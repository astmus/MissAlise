namespace MissAlise.Entities.YandexTracker
{
	public abstract record UriEntity<TIdentifier> : Entity<TIdentifier>
	{
		public virtual Uri Self { get; set; }
	}
}