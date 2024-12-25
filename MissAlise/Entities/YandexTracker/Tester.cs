using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Tester : UriEntity<ulong>
	{
		[JsonProperty("display")]
		public string Display { get; set; }

		[JsonProperty("cloudUid")]
		public string CloudUid { get; set; }
	}
}
