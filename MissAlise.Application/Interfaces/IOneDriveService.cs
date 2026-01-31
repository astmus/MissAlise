using MissAlise.Application.Common;
using MissAlise.Entities.Identity;
using MissAlise.Entities.Media;
using MissAlise.ValueObjects;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{		
		Uri CreateAuthorizeLink(object stateIdentifier);
		Task<string> HandleDeltaDriveItemsAsync(Action<MediaItem> handleDelegate, CancellationToken cancel);
		Task<string> HandleActualDriveItemsAsync(Action<MediaItem> handleDelegate, CancellationToken cancel);
		Task<Result<AccessInformation>> RefreshUserAccessTokenAsync(UserId userId, AccessInformation currentAccess, CancellationToken cancel);
		Task<Stream?> GetItemContent(string parentId, string itemId, CancellationToken cancel);
	}	
}
