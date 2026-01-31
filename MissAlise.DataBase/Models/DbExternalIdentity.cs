namespace MissAlise.DataBase.Models;

public sealed class DbExternalIdentity
{
	public string Scheme { get; set; } = null!;      // "telegram" | "web" | "azuread"
	public string ExternalId { get; set; } = null!;  // tg id / sub / oid / graph user id

	// опционально, но очень полезно:
	public bool IsPrimary { get; set; }              // какой вход “главный”
	public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

	public string? DisplayName { get; set; }         // tg username / email / name
}
