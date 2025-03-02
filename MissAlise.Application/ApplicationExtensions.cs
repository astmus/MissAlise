using MissAlise.Application.Common;

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
