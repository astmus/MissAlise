using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public partial record Issue : UriEntity<string>
	{
		[JsonPropertyName("id")]
		public virtual string Id { get; set; }
		[JsonProperty("key")]
		public virtual string Key { get; set; }

		[JsonProperty("version")]
		public virtual int Version { get; set; }

		[JsonProperty("summary")]
		public virtual string Summary { get; set; }

		[JsonProperty("statusStartTime")]
		public virtual DateTime? StatusStartTime { get; set; }

		[JsonProperty("updatedBy")]
		public virtual ViewEntity UpdatedBy { get; set; }

		[JsonProperty("statusType")]
		public virtual StatusType StatusType { get; set; }

		[JsonProperty("description")]
		public virtual string Description { get; set; } = "Описание не указано.";

		[JsonProperty("boards")]
		public virtual List<Board> Boards { get; set; }

		[JsonProperty("type")]
		public virtual EntityType Type { get; set; }

		[JsonProperty("priority")]
		public virtual Priority Priority { get; set; }

		[JsonProperty("createdAt")]
		public virtual DateTime CreatedAt { get; set; }

		[JsonProperty("createdBy")]
		public virtual ViewEntity CreatedBy { get; set; }

		[JsonProperty("commentWithoutExternalMessageCount")]
		public virtual int CommentWithoutExternalMessageCount { get; set; }

		[JsonProperty("shagidljavosproizvedenija")]
		public virtual string Shagidljavosproizvedenija { get; set; }

		[JsonProperty("votes")]
		public virtual int Votes { get; set; }

		[JsonProperty("commentWithExternalMessageCount")]
		public virtual int CommentWithExternalMessageCount { get; set; }

		[JsonProperty("queue")]
		public virtual Queue Queue { get; set; }

		[JsonProperty("updatedAt")]
		public virtual DateTime UpdatedAt { get; set; }

		[JsonProperty("status")]
		public virtual Status Status { get; set; }

		[JsonProperty("favorite")]
		public virtual bool Favorite { get; set; }

		[JsonProperty("project")]
		public virtual Project Project { get; set; }

		[JsonProperty("previousStatusLastAssignee")]
		public virtual Assignee PreviousStatusLastAssignee { get; set; }

		[JsonProperty("deadline")]
		public virtual string Deadline { get; set; }

		[JsonProperty("lastCommentUpdatedAt")]
		public virtual DateTime? LastCommentUpdatedAt { get; set; }

		[JsonProperty("originalEstimation")]
		public virtual string OriginalEstimation { get; set; }

		[JsonProperty("spent")]
		public virtual string Spent { get; set; }

		[JsonProperty("start")]
		public virtual string Start { get; set; }

		[JsonProperty("sla")]
		public virtual List<Sla> Sla { get; set; }

		[JsonProperty("followers")]
		public virtual List<Follower> Followers { get; set; }

		[JsonProperty("assignee")]
		public virtual Assignee Assignee { get; set; }

		[JsonProperty("previousStatus")]
		public virtual Status PreviousStatus { get; set; }

		[JsonProperty("pendingReplyFrom")]
		public virtual List<ViewEntity> PendingReplyFrom { get; set; }

		[JsonProperty("testirovschik")]
		public virtual List<Tester> Testirovschik { get; set; }

		[JsonProperty("aliases")]
		public virtual List<string> Aliases { get; set; }

		[JsonProperty("previousQueue")]
		public virtual Queue PreviousQueue { get; set; }

		[JsonProperty("lastQueue")]
		public virtual Queue LastQueue { get; set; }

		[JsonProperty("sprint")]
		public virtual List<Sprint> Sprint { get; set; }

		[JsonProperty("resolvedBy")]
		public virtual ViewEntity ResolvedBy { get; set; }

		[JsonProperty("unique")]
		public virtual string Unique { get; set; }

		[JsonProperty("checklistDone")]
		public virtual int? ChecklistDone { get; set; }

		[JsonProperty("checklistTotal")]
		public virtual int? ChecklistTotal { get; set; }

		[JsonProperty("checklistItems")]
		public virtual List<CheckListItem> ChecklistItems { get; set; }

		[JsonProperty("estimation")]
		public virtual string Estimation { get; set; }
	}
}
