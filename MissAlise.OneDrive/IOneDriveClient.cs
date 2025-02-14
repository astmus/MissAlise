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

		[Get("/root/delta?$select=name,folder,parentReference,size,id,createdDateTime,file,@microsoft.graph.downloadUrl,fileSystemInfo,photo,image,audio,video")]
		Task<ApiResponse<DriveItemDeltaGetResponse>> RootDelta([Property] IHandleContext ctx, CancellationToken cancel);

		[Get("/root/delta")]
		Task<ApiResponse<DriveItemDeltaGetResponse>> DeltaShift(string token, [Property] IHandleContext ctx, CancellationToken cancel);
	}
}
