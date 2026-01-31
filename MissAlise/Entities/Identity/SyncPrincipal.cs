using MissAlise.ValueObjects;

namespace MissAlise.Entities.Identity
{
	/// <summary>
	/// Контекст текущего пользователя для sync-запроса: профиль + OAuth-креды (из отдельного хранилища).
	/// </summary>
	public sealed record SyncPrincipal(UserProfile Profile, AccessInformation Access);
}
