namespace MissAlise.AppHost.Background
{
	public record SyncBackgroundTask(ushort BatchSize = 128, DateTime LastUpdated = new());
	public record UpdateUsersBackgroundTask(ushort BatchSize = 32, DateTime LastUpdated = new());
	public record UpdateIssuesBackgroundTask(ushort BatchSize = 32, DateTime LastUpdated = new());
	public record CreatedIssuesBackgroundTask(ushort BatchSize = 32, DateTime LastUpdated = new());
}
