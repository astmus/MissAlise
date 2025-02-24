using Microsoft.Graph;
using Microsoft.Graph.Models;
using MissAlise.Application.Abstractions;
using MissAlise.Application.Models;
using MissAlise.Entities.OneDrive;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{
		IOrderedQueryable<DriveItem> DataContext { get; }
		DataSynchronizator GetSynchronizator();
		Task<User> GetOwnerInfo(CancellationToken cancel);
		Uri CreateAuthorizeLink(object stateIdentifier);
		Task<IEnumerable<ItemInfo>> GetRootItems(CancellationToken cancel);
		Task<Result<AppUser>> RefreshUserAccessTokenAsync(AppUser user, CancellationToken cancel);
	}

	public abstract class DataSynchronizator : IAsyncEnumerable<ItemInfo>
	{
		public abstract IAsyncEnumerator<ItemInfo> GetAsyncEnumerator(CancellationToken cancellationToken = default);
	}
}
