
using System.Text.Json.Serialization;

namespace MissAlise.Entities.YandexTracker
{
	//[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public record IamToken
	{
		[JsonPropertyName("iamToken")] public string iamToken { get; set; }
		[JsonPropertyName("expiresAt")] public DateTime expiresAt { get; set; }
	}
}
