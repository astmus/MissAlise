using MissAlise.Application.Models;

namespace MissAlise.Application.Services.Authentication
{
	public interface IAuthenticationService
	{
		Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password);
		Task<RegisterUserResponse> AddUserAsync(AppUser pendingUser);
		Task<bool> UserExistsAsync(AppUser user);
		Task<bool> LoginUserAsync(string username, string password);
		Task LogoutUserAsync();
	}
}
