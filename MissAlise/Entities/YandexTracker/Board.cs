using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Board : Entity<int>
	{
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
