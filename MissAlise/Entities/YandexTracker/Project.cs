using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public record Project : UriEntity<int>
	{
		[JsonPropertyName("id")]
		public int Id { get => base.Identifier; set => base.Identifier = value; }

		public double LeftHours { get; set; }

		[JsonPropertyName("key")]
		public string Key { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("version")]
		public int Version { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("lead")]
		public User Lead { get; set; }

		[JsonPropertyName("assignAuto")]
		public bool AssignAuto { get; set; }

		[JsonPropertyName("defaultType")]
		public string DefaultType { get; set; }

		[JsonPropertyName("defaultPriority")]
		public string DefaultPriority { get; set; }

		[JsonPropertyName("allowExternalMailing")]
		public bool AllowExternalMailing { get; set; }

		[JsonPropertyName("denyVoting")]
		public bool DenyVoting { get; set; }

		[JsonPropertyName("denyConductorAutolink")]
		public bool DenyConductorAutolink { get; set; }

		[JsonPropertyName("denyTrackerAutolink")]
		public bool DenyTrackerAutolink { get; set; }

		[JsonPropertyName("useComponentPermissionsIntersection")]
		public bool UseComponentPermissionsIntersection { get; set; }

		[JsonPropertyName("addSummoneeToIssueAccess")]
		public bool AddSummoneeToIssueAccess { get; set; }

		[JsonPropertyName("addCommentAuthorToIssueFollowers")]
		public bool AddCommentAuthorToIssueFollowers { get; set; }

		[JsonPropertyName("workflowActionsStyle")]
		public string WorkflowActionsStyle { get; set; }

		[JsonPropertyName("useLastSignature")]
		public bool UseLastSignature { get; set; }
	}
}
