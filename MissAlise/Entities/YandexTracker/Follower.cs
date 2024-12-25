using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Follower : ViewEntity
	{
		[JsonProperty("cloudUid")]
		public string CloudUid { get; set; }

		[JsonProperty("passportUid")]
		public int? PassportUid { get; set; }
	}
}
