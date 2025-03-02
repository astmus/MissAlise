using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MissAlise.Application.Common;

namespace MissAlise.Application.Services.User
{
	public class UserService : IUserService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IHttpContextAccessor _httpContextAccessor;        
		private readonly RoleManager<IdentityRole> _roleManager;

		public UserService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;            
			_httpContextAccessor = httpContextAccessor;
			_roleManager = roleManager;            
		}

		public async Task AddRoleToUserAsync(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user is null)
				throw new Exception("User not found.");
			if (!await _roleManager.RoleExistsAsync(roleName))
			{
				var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
				if (!roleResult.Succeeded)
					throw new Exception("Failed to create role.");
			}
			var result = await _userManager.AddToRoleAsync(user, roleName);
			if (!result.Succeeded)
				throw new Exception("Failed to add user role.");
		}

		public async Task<string> GetCurrentUserIdAsync()
		{
			var user = await GetCurrentUserAsync();
			if (user is null)
				throw new UnauthorizedAccessException("User not authorized");
			return user.Id;
		}

		public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user is null)
				return Enumerable.Empty<string>();

			var roles = await _userManager.GetRolesAsync(user);
			return roles.ToList();
		}

		public async Task<bool> IsCurrentUserInRoleAsync(string role)
		{
			var user = await GetCurrentUserAsync();
			var result = user is not null && await _userManager.IsInRoleAsync(user, role);
			return result;
		}

		public async Task RemoveRoleFromUserAsync(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user is null)
				throw new Exception("User not found.");
			var result = await _userManager.RemoveFromRoleAsync(user, roleName);
			if (!result.Succeeded)
				throw new Exception("Failed to remove user role.");
		}

		private async Task<AppUser?> GetCurrentUserAsync()
		{
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext is null || httpContext.User is null)
				return null;
			return await _userManager.GetUserAsync(httpContext.User);
		}
	}
}
