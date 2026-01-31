namespace MissAlise.DataBase.Bulk;

using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using MissAlise.Entities.Media;

internal static class PostgresCopyMerge
{
	public static async Task BulkUpsertViaCopyAsync(
		DbContext db,
		IReadOnlyCollection<MediaItem> items,
		CancellationToken ct)
	{
		if (items.Count == 0)
			return;

		var conn = (NpgsqlConnection)db.Database.GetDbConnection();
		if (conn.State != ConnectionState.Open)
			await conn.OpenAsync(ct);

		await using var tx = await conn.BeginTransactionAsync(ct);

		try
		{
			// 1️⃣ TEMP TABLE
			await using (var cmd = new NpgsqlCommand(@"
CREATE TEMP TABLE temp_media_items (
  id uuid,
  kind int,
  provider int,
  drive_id text,
  remote_item_id text,

  name text,
  path text,

  size_bytes bigint,
  mime_type text,

  created_at timestamptz,
  modified_at timestamptz,
  taken_at timestamptz,

  width int,
  height int,
  duration_seconds double precision,

  hash_algo text,
  hash_value text,

  is_deleted boolean
) ON COMMIT DROP;", conn, tx))
			{
				await cmd.ExecuteNonQueryAsync(ct);
			}

			// 2️⃣ COPY
			await using (var importer = await conn.BeginBinaryImportAsync(@"
COPY temp_media_items (
  id, kind, provider, drive_id, remote_item_id,
  name, path, size_bytes, mime_type,
  created_at, modified_at, taken_at,
  width, height, duration_seconds,
  hash_algo, hash_value, is_deleted
) FROM STDIN (FORMAT BINARY);", ct))
			{
				foreach (var it in items)
				{
					await importer.StartRowAsync(ct);

					importer.Write(it.Id.Value, NpgsqlDbType.Uuid);
					importer.Write((int)it.Kind, NpgsqlDbType.Integer);
					importer.Write((int)it.Provider, NpgsqlDbType.Integer);
					importer.Write(it.DriveId ?? "", NpgsqlDbType.Text);
					importer.Write(it.RemoteItemId, NpgsqlDbType.Text);

					importer.Write(it.Name, NpgsqlDbType.Text);
					importer.Write(it.Path.Value ?? "", NpgsqlDbType.Text);

					importer.Write(it.Size.Bytes, NpgsqlDbType.Bigint);
					importer.Write(it.MimeType.Value, NpgsqlDbType.Text);

					importer.Write(it.Timestamps.CreatedAt, NpgsqlDbType.TimestampTz);
					importer.Write(it.Timestamps.ModifiedAt, NpgsqlDbType.TimestampTz);

					if (it.Timestamps.TakenAt.HasValue)
						importer.Write(it.Timestamps.TakenAt.Value, NpgsqlDbType.TimestampTz);
					else
						importer.WriteNull();

					if (it.Dimensions.HasValue)
					{
						importer.Write(it.Dimensions.Value.Width, NpgsqlDbType.Integer);
						importer.Write(it.Dimensions.Value.Height, NpgsqlDbType.Integer);
					}
					else
					{
						importer.WriteNull();
						importer.WriteNull();
					}

					if (it.Duration.HasValue)
						importer.Write(it.Duration.Value.Value.TotalSeconds, NpgsqlDbType.Double);
					else
						importer.WriteNull();

					if (it.Hash is not null)
					{
						importer.Write(it.Hash.Algorithm, NpgsqlDbType.Text);
						importer.Write(it.Hash.Value, NpgsqlDbType.Text);
					}
					else
					{
						importer.WriteNull();
						importer.WriteNull();
					}

					importer.Write(it.IsDeleted, NpgsqlDbType.Boolean);
				}

				await importer.CompleteAsync(ct);
			}

			await using (var cmd = new NpgsqlCommand(@"
INSERT INTO media_items (
  id, kind, provider, drive_id, remote_item_id,
  name, path, size_bytes, mime_type,
  created_at, modified_at, taken_at,
  width, height, duration_seconds,
  hash_algo, hash_value, is_deleted
)
SELECT
  id, kind, provider, drive_id, remote_item_id,
  name, path, size_bytes, mime_type,
  created_at, modified_at, taken_at,
  width, height, duration_seconds,
  hash_algo, hash_value, is_deleted
FROM temp_media_items
ON CONFLICT (provider, drive_id, remote_item_id)
DO UPDATE SET
  kind = EXCLUDED.kind,
  name = EXCLUDED.name,
  path = EXCLUDED.path,
  size_bytes = EXCLUDED.size_bytes,
  mime_type = EXCLUDED.mime_type,
  created_at = EXCLUDED.created_at,
  modified_at = EXCLUDED.modified_at,
  taken_at = EXCLUDED.taken_at,
  width = EXCLUDED.width,
  height = EXCLUDED.height,
  duration_seconds = EXCLUDED.duration_seconds,
  hash_algo = EXCLUDED.hash_algo,
  hash_value = EXCLUDED.hash_value,
  is_deleted = EXCLUDED.is_deleted;", conn, tx))
			{
				await cmd.ExecuteNonQueryAsync(ct);
			}

			await tx.CommitAsync(ct);
		}
		catch
		{
			await tx.RollbackAsync(ct);
			throw;
		}
	}
}
