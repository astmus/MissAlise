using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record CheckListItem : Entity
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("textHtml")]
		public string TextHtml { get; set; }

		[JsonProperty("checked")]
		public bool Checked { get; set; }

		[JsonProperty("assignee")]
		public Assignee Assignee { get; set; }

		[JsonProperty("deadline")]
		public Deadline Deadline { get; set; }

		[JsonProperty("checklistItemType")]
		public string ChecklistItemType { get; set; }
	}
}
