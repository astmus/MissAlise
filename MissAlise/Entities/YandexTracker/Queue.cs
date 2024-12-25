namespace MissAlise.Entities.YandexTracker
{
	public record Queue : UriEntity<int>
	{
		public int id { get; set; }
		public string key { get; set; }
		public int version { get; set; }
		public string name { get; set; }
		public string Display { get; set; }
		public string description { get; set; }
		//public Lead lead { get; set; }
		public bool assignAuto { get; set; }
		//public Defaulttype defaultType { get; set; }
		//public Defaultpriority defaultPriority { get; set; }
		public bool allowExternalMailing { get; set; }
		public bool denyVoting { get; set; }
		public bool denyConductorAutolink { get; set; }
		public bool denyTrackerAutolink { get; set; }
		public bool useComponentPermissionsIntersection { get; set; }
		public bool addSummoneeToIssueAccess { get; set; }
		public bool addCommentAuthorToIssueFollowers { get; set; }
		public string workflowActionsStyle { get; set; }
		public bool useLastSignature { get; set; }
	}
}
