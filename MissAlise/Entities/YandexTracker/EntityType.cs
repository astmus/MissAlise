using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record EntityType : UriEntity<object>
	{
		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("display")]
		public string Display { get; set; }
	}
}
