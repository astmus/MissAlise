using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record ViewEntity : UriEntity<ulong>
	{
		[JsonProperty("display")]
		public string Display { get; set; }
	}
}
