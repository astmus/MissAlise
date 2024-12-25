using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record IssueType : UriEntity<int>
	{
		public int Id { get; set; }
		public int Version { get; set; }
		public string Key { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

}
