using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Deadline
	{
		[JsonProperty("date")]
		public DateTime Date { get; set; }

		[JsonProperty("deadlineType")]
		public string DeadlineType { get; set; }

		[JsonProperty("isExceeded")]
		public bool IsExceeded { get; set; }
	}
}
