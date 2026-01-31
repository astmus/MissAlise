namespace MissAlise.DataBase.Models
{
	public sealed class DbTelegramProfile
	{
		/// <summary>
		/// Telegram user id (храним как string — безопаснее и универсальнее)
		/// </summary>
		public string Id { get; set; } = null!;

		public string? Username { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
	}
}
