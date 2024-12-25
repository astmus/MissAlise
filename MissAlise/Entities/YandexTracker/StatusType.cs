using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record StatusType
	{
		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("order")]
		public int Order { get; set; }
	}
}
