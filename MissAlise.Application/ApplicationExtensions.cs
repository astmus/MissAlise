using MissAlise.Application.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise
{
	public static class ApplicationExtensions
	{
		public static bool HasExpiredCredentials(this AppUser user)
		{
			return DateTimeOffset.UtcNow > user.AccessData?.ExpiredAfter;
		}
	}
}
