using Newtonsoft.Json;
namespace MissAlise.Entities.YandexTracker
{
	public partial class LinkFrom
	{
		[JsonProperty("type")]
		public TypeClass Type { get; set; }

		[JsonProperty("direction")]
		public string Direction { get; set; }

		[JsonProperty("object")]
		public Issue Object { get; set; }
	}
}
