using MissAlise.Application.Interfaces;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using Refit;

namespace MissAlise.OneDrive
{
	public interface IOneDriveClient
	{
		[Get("/root/children")]
		Task<ApiResponse<IEnumerable<DriveItem>>> RootChildren([Property] IHandleContext ctx, CancellationToken cancel);

		[Get("/root/delta?$select=name,folder,parentReference,size,id,createdDateTime,file,@microsoft.graph.downloadUrl,fileSystemInfo,photo,image,audio,video")]
		Task<ApiResponse<DriveItemsDelta>> RootDelta([Property] IHandleContext ctx, CancellationToken cancel);

		[Get("/root/delta")]
		Task<ApiResponse<DriveItemsDelta>> DeltaShift(string token, [Property] IHandleContext ctx, CancellationToken cancel);

		[Get("/root/delta?{query}")]
		[QueryUriFormat(UriFormat.Unescaped)]
		Task<ApiResponse<DriveItemsDelta>> DeltaQuery(string query, [Property] IHandleContext ctx, CancellationToken cancel);
	}
}
