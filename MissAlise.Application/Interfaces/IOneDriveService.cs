using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{		
		DataSynchronizator GetSynchronizator();
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(string stateIdentifier);
		Task<IEnumerable<Item>> GetRootItems(CancellationToken cancel);
	}

	public abstract class DataSynchronizator : IAsyncEnumerable<Item>
	{
		public abstract IAsyncEnumerator<Item> GetAsyncEnumerator(CancellationToken cancellationToken = default);
	}
}
