using MissAlise.Entities.Identity;

namespace MissAlise.DataBase.Models
{
	internal sealed class DbAccessCredentials
	{
		public Guid OwnerId { get; set; }
		public string TokenType { get; set; } = string.Empty;
		public string Scope { get; set; } = string.Empty;
		public int ExpiresIn { get; set; }
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
		public string IdToken { get; set; } = string.Empty;
		public DateTimeOffset ExpiredAfter { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
