namespace MissAlise.Entities.Identity
{
	public class AccessInformation
	{
		public string TokenType { get; set; }
		public string Scope { get; set; }
		public int ExpiresIn { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string IdToken { get; set; }
		public DateTimeOffset ExpiredAfter { get; set; }
	}
}
