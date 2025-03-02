using MissAlise.Application.Interfaces;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using Refit;

namespace MissAlise.OneDrive
{
	[Headers("Authorization: Bearer")]
	internal interface IOneDriveClient
	{
		[Get("/root/children")]
		Task<ApiResponse<IEnumerable<DriveItem>>> RootChildren([Authorize] string authToken, CancellationToken cancel);

		[Get("/root/delta")]//?$select=name,folder,parentReference,size,id,createdDateTime,file,@microsoft.graph.downloadUrl,fileSystemInfo,photo,image,audio,video
		Task<ApiResponse<DriveItemsDelta>> RootDelta([Authorize] string authToken, CancellationToken cancel);

		[Get("/root/delta")]
		Task<ApiResponse<DriveItemsDelta>> RootDeltaSelect([Authorize] string authToken, [AliasAs("$select")]string select, CancellationToken cancel);

		[Get("/root/delta")]
		Task<ApiResponse<DriveItemsDelta>> DeltaShift(string token, [Authorize] string authToken, CancellationToken cancel);

		[Get("/root/delta?{query}")]
		[QueryUriFormat(UriFormat.Unescaped)]
		Task<ApiResponse<DriveItemsDelta>> DeltaQuery(string query, [Authorize] string authToken, CancellationToken cancel);
	}
}
