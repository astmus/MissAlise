using MissAlise.Application.Interfaces;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using Refit;

namespace MissAlise.OneDrive
{
	public interface IOneDriveClient
	{
		[Get("/root/children")]
		Task<ApiResponse<IEnumerable<DriveItem>>> RootItems([Property] IHandleContext ctx, CancellationToken cancel);

		[Get("/root/delta")]
		Task<ApiResponse<DeltaGetResponse>> RootDelta([Property] IHandleContext ctx, CancellationToken cancel);
	}
}
