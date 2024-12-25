using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record User : UriEntity<ulong>
	{
		[JsonProperty("trackerUid")]
		[JsonPropertyName("trackerUid")]
		public virtual ulong Id { get; set; }

		[JsonProperty("login")]
		public virtual string Login { get; set; }

		[JsonProperty("passportUid")]
		[JsonPropertyName("passportUid")]
		public virtual long PassportUid { get; set; }

		[JsonProperty("cloudUid")]
		public virtual string CloudUid { get; set; }

		[JsonProperty("firstName")]
		public virtual string FirstName { get; set; }

		[JsonProperty("lastName")]
		public virtual string LastName { get; set; }

		[JsonProperty("display")]
		public virtual string Display { get; set; }

		[JsonProperty("email")]
		public virtual string Email { get; set; }

		[JsonProperty("external")]
		public virtual bool External { get; set; }

		[JsonProperty("hasLicense")]
		public virtual bool HasLicense { get; set; }

		[JsonProperty("dismissed")]
		public virtual bool Dismissed { get; set; }

		[JsonProperty("useNewFilters")]
		public virtual bool UseNewFilters { get; set; }

		[JsonProperty("disableNotifications")]
		public virtual bool DisableNotifications { get; set; }

		[JsonProperty("firstLoginDate")]
		public virtual string FirstLoginDate { get; set; }

		[JsonProperty("lastLoginDate")]
		public virtual string LastLoginDate { get; set; }

		[JsonProperty("welcomeMailSent")]
		public virtual bool WelcomeMailSent { get; set; }

		[JsonProperty("sources")]
		public virtual List<string> Sources { get; set; }
	}
}