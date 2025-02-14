using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Interfaces
{
	public interface IAuthorizationCompleter
	{
		Task AuthorizationCompleted(string state, UserCredentials? credentials, CancellationToken cancel);
	}
}
