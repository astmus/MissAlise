using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(string stateIdentifier);
	}
}
