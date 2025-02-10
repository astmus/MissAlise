using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using Refit;

namespace MissAlise.OneDrive
{
	public interface IOneDriveClient
	{
		[Get("/root/children")]
		Task<ApiResponse<IEnumerable<DriveItem>>> RootItems(CancellationToken cancel);

		[Get("/root/delta")]
		Task<ApiResponse<DeltaGetResponse>> RootDelta(CancellationToken cancel);
	}
}
