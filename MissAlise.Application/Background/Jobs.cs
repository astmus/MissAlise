using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Background
{
	public record SyncDataJob(ushort BatchSize = 128, DateTime LastUpdated = new());
	public record SyncOneDriveJob(UserProfile profile, DateTime LastUpdated = new());
	public record UpdateUsersJob(ushort BatchSize = 32, DateTime LastUpdated = new());
	public record UpdateIssuesJob(ushort BatchSize = 32, DateTime LastUpdated = new());
	public record CreatedIssuesJob(ushort BatchSize = 32, DateTime LastUpdated = new());
}
