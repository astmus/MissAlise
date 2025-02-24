using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MissAlise.Application.Models;

namespace MissAlise.Application.Services.Authentication
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		public AuthenticationService(SignInManager<AppUser> signInManager)
		{
			_userManager = signInManager.UserManager;
			_signInManager = signInManager;
		}
		public async Task<bool> LoginUserAsync(string username, string password)
		{
			var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
			return result.Succeeded;
		}

		public async Task LogoutUserAsync()
		{
			await _signInManager.SignOutAsync();
		}

		public async Task<RegisterUserResponse> RegisterUserAsync(string username, string email, string password)
		{
			var user = new AppUser
			{
				UserName = username,
				Email = email,
				EmailConfirmed = false
			};

			var result = await _userManager.CreateAsync(user, password);
			if (result.Succeeded)
				await _userManager.AddToRoleAsync(user, "Reader");
			var response = new RegisterUserResponse
			{
				Succeeded = result.Succeeded,
				Errors = result.Errors.Select(e => e.Description).ToList()
			};
			return response;
		}

		public async Task<RegisterUserResponse> AddUserAsync(AppUser pendingUser)
		{			
			var result = await _userManager.CreateAsync(pendingUser);			
			var response = new RegisterUserResponse
			{
				Succeeded = result.Succeeded,
				Errors = result.Errors.Select(e => e.Description).ToList()
			};
			return response;
		}

		public Task<bool> UserExistsAsync(AppUser user) 
			=> _userManager.Users.AnyAsync(u=> u.Id == user.Id);
	}
}
