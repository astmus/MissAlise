using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IAuthorizationCompleter
	{
		Task AuthorizationCompleted(string state, Credentials? credentials, CancellationToken cancel);
	}
}
