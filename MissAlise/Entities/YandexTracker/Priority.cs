using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Priority : UriEntity<int>
	{
		[JsonProperty("key")]
		public string Key { get; set; }
		public int Version { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Order { get; set; }
	}
}
