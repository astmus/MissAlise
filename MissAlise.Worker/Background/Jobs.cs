namespace MissAlise.Worker.Background
{
	public record SyncDataJob(ushort BatchSize = 128, DateTime LastUpdated = new());
	public record UpdateUsersJob(ushort BatchSize = 32, DateTime LastUpdated = new());
	public record UpdateIssuesJob(ushort BatchSize = 32, DateTime LastUpdated = new());
	public record CreatedIssuesJob(ushort BatchSize = 32, DateTime LastUpdated = new());
}
