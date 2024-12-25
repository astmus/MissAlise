using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Status : UriEntity<uint>
	{
		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("display")]
		public string Display { get; set; }
		public uint id { get; set; }
		public int version { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public int order { get; set; }
		public string type { get; set; }
	}

}
