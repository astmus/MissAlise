using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record WorkLog : UriEntity<int>
	{
		[JsonProperty("id")]
		public virtual int Id { get; set; }

		[JsonProperty("version")]
		public int Version { get; set; }

		[JsonProperty("issue")]
		public Issue Issue { get; set; }

		[JsonProperty("createdBy")]
		public User CreatedBy { get; set; }

		[JsonProperty("updatedBy")]
		public ViewEntity UpdatedBy { get; set; }

		[JsonProperty("createdAt")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("updatedAt")]
		public DateTime UpdatedAt { get; set; }

		[JsonProperty("start")]
		public DateTime Start { get; set; }

		[JsonProperty("duration")]
		public string Duration { get; set; }

		[JsonProperty("comment")]
		public string Comment { get; set; }
	}
}
