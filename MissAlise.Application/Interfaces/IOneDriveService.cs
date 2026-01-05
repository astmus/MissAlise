using MissAlise.Application.Common;
using MissAlise.Entities.OneDrive;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{		
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(object stateIdentifier);
		Task<string> HandleDeltaDriveItemsAsync(Action<ItemInfo> handleDelegate, CancellationToken cancel);
		Task<string> HandleActualDriveItemsAsync(Action<ItemInfo> handleDelegate, CancellationToken cancel);
		Task<Result<AppUser>> RefreshUserAccessTokenAsync(AppUser user, CancellationToken cancel);
		Task<Stream?> GetItemContent(string parentId, string itemId, CancellationToken cancel);
	}	
}
