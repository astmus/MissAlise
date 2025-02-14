using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{
		//Task<DeltaGetResponse> GetRootItems(CancellationToken cancel);
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(string stateIdentifier);
		Task<IEnumerable<Item>> GetRootItems(CancellationToken cancel);
	}	
}
