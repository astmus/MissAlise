using System.Linq.Expressions;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MissAlise.Application.Common;
using MissAlise.Entities.OneDrive;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.Application.Interfaces
{
	public interface IOneDriveService
	{		
		DataSynchronizator GetSynchronizator(Expression<Func<DriveItem, object>> select = null);
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
