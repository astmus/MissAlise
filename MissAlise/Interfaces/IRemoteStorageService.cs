using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IRemoteStorageService
	{
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(string stateIdentifier);
	}

	
}
