namespace MissAlise.Application.Services.User
{
    public interface IUserService
    {
        Task<string> GetCurrentUserIdAsync();
        Task<bool> IsCurrentUserInRoleAsync(string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task AddRoleToUserAsync(string userId, string roleName);
        Task RemoveRoleFromUserAsync(string userId, string roleName);
    }
}
