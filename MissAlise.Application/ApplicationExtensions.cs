using MissAlise.Application.Common;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.Application
{
	public static class ApplicationExtensions
	{
		public static bool HasExpiredCredentials(this AppUser user)
		{
			return DateTimeOffset.UtcNow > user.AccessData?.ExpiredAfter;
		}
	}
}
