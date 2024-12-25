using Newtonsoft.Json;
namespace MissAlise.Entities.YandexTracker
{
	public partial class FromElement
	{
		[JsonProperty("id")]
		public long Id { get; set; }
	}
}
