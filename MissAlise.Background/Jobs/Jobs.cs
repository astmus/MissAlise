namespace MissAlise.Background.Jobs
{
	public record ExecuteUpdateUsersTask
	{
		public ushort BatchSize { get; set; }
	}

	public record ExecuteUpdateIssuesTask
	{
		public int TaskCount { get; set; }
	}

	public record ExecuteTestTask2
	{
	}

	public record ExecuteTestTask3
	{
	}
}
