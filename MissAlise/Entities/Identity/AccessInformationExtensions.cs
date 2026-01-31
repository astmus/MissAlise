namespace MissAlise.Entities.Identity
{
	public static class AccessInformationExtensions
	{
		public static bool IsExpired(this AccessInformation? access)
			=> access is null || DateTimeOffset.UtcNow > access.ExpiredAfter;
	}
}
