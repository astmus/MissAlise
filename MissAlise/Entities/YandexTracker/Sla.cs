using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Sla : Entity
	{
		[JsonProperty("settingsId")]
		public int SettingsId { get; set; }

		[JsonProperty("clockStatus")]
		public string ClockStatus { get; set; }

		[JsonProperty("violationStatus")]
		public string ViolationStatus { get; set; }

		[JsonProperty("warnThreshold")]
		public object WarnThreshold { get; set; }

		[JsonProperty("failedThreshold")]
		public int? FailedThreshold { get; set; }

		[JsonProperty("warnAt")]
		public DateTime? WarnAt { get; set; }

		[JsonProperty("failAt")]
		public DateTime? FailAt { get; set; }

		[JsonProperty("startedAt")]
		public DateTime? StartedAt { get; set; }

		[JsonProperty("pausedAt")]
		public DateTime? PausedAt { get; set; }

		[JsonProperty("stoppedAt")]
		public DateTime? StoppedAt { get; set; }

		[JsonProperty("pausedDuration")]
		public int PausedDuration { get; set; }

		[JsonProperty("toFailTimeWorkDuration")]
		public object ToFailTimeWorkDuration { get; set; }

		//[JsonProperty("spent")]		
		//public int? Spent { get; set; }

		[JsonProperty("previousSLAs")]
		public List<object> PreviousSLAs { get; set; }

		[JsonProperty("startShiftedByPause")]
		public DateTime? StartShiftedByPause { get; set; }

		[JsonProperty("failIn")]
		public object FailIn { get; set; }
	}
}
