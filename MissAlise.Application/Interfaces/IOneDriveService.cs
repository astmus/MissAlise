using MissAlise.Application.Common;
using MissAlise.Entities.OneDrive;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{		
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(object stateIdentifier);
		Task<IEnumerable<ItemInfo>> GetRootItems(CancellationToken cancel);
		Task<Result<AppUser>> RefreshUserAccessTokenAsync(AppUser user, CancellationToken cancel);
	}	
}
