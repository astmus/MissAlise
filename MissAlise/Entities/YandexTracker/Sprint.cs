using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Sprint : UriEntity<object>
	{
		[JsonProperty("display")]
		public string Display { get; set; }
	}
}
