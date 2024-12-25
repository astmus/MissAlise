using Newtonsoft.Json;
namespace MissAlise.Entities.YandexTracker
{
	public partial class Link
	{
		[JsonProperty("from")]
		public LinkFrom From { get; set; }

		[JsonProperty("to")]
		public LinkFrom To { get; set; }
	}
}
