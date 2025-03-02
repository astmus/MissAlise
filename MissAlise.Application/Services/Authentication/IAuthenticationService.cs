using MissAlise.Application.Common;

namespace MissAlise.Application.Services.Authentication
{
	public interface IAuthenticationService
	{
		Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password);
		Task<RegisterUserResponse> AddUserAsync(AppUser pendingUser);
		Task<bool> UserExistsAsync(AppUser user);
		Task<bool> LoginUserAsync(string username, string password);
		Task<AppUser> LoginUserAsync(string userId);
		Task<bool> UpdateUserAsync(AppUser user);
		Task LogoutUserAsync();
	}
}
