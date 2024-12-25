using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public abstract record UriEntity<TIdentifier> : Entity<TIdentifier>
	{
		public virtual Uri Self { get; set; }
	}
}