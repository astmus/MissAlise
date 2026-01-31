using MongoDB.Bson;

namespace MissAlise.DataBase.Models
{
	internal sealed class DbUserProfile
	{
		public ObjectId Id { get; set; }

		/// <summary>
		/// ВНУТРЕННИЙ идентификатор пользователя в системе MissAlise
		/// Используется ВЕЗДЕ вне Mongo (Postgres, Domain, Media, Sync)
		/// </summary>
		public Guid OwnerId { get; set; }

		public List<DbExternalIdentity> Identities { get; set; } = new();

		public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
		public DateTimeOffset? UpdatedAt { get; set; }
	}
}
