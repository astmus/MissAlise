using MissAlise.Entities.OneDrive;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{		
		DataSynchronizator GetSynchronizator();
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(string stateIdentifier);
		Task<IEnumerable<ItemInfo>> GetRootItems(CancellationToken cancel);
	}

	public abstract class DataSynchronizator : IAsyncEnumerable<ItemInfo>
	{
		public abstract IAsyncEnumerator<ItemInfo> GetAsyncEnumerator(CancellationToken cancellationToken = default);
	}
}
