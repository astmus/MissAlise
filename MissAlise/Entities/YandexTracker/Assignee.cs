using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Assignee : User
	{
		[JsonProperty("gender")]
		public string Gender { get; set; }

		[JsonProperty("robot")]
		public bool Robot { get; set; }

		[JsonProperty("effectiveLicenseType")]
		public int EffectiveLicenseType { get; set; }
	}
}
