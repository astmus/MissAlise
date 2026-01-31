using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace MissAlise.DataBase.Repositories;

/// <summary>
/// Быстрый bulk-upsert в PostgreSQL через INSERT ... ON CONFLICT DO UPDATE.
/// Предполагается таблица media_items и уникальный индекс (provider, drive_id_norm, remote_item_id).
///
/// ВАЖНО: drive_id_norm должен быть нормализован: NULL -> "" (пустая строка),
/// иначе уникальность с NULL может вести себя неожиданно.
/// </summary>
public sealed partial class MediaRepository
{
    // Модель, которую ты уже используешь в репозитории/DbContext:
    // - можно заменить на твою DbMediaItem
    // - тут минимальный контракт (анонимная проекция) чтобы не тянуть домен в DataBase слой
    public sealed record UpsertRow(
        Guid Id,
        int Provider,
        string DriveIdNorm,
        string RemoteItemId,
        int Kind,
        string Name,
        string Path,
        long SizeBytes,
        string MimeType,
        DateTimeOffset CreatedAt,
        DateTimeOffset ModifiedAt,
        DateTimeOffset? TakenAt,
        int? Width,
        int? Height,
        double? DurationSeconds,
        string? HashAlgo,
        string? HashValue,
        bool IsDeleted);

    /// <summary>
    /// Bulk-upsert пачки записей. Делает 1 SQL на батч.
    /// </summary>
    public async Task<int> BulkUpsertAsync(
        IReadOnlyCollection<UpsertRow> rows,
        CancellationToken ct = default)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        if (rows.Count == 0) return 0;

        // Опционально: батчируй, если боишься лимитов по параметрам.
        // PostgreSQL ограничивает число параметров косвенно (практически - размер запроса/память).
        // По умолчанию оставляем единым батчем.
        var sql = BuildUpsertSql(rows.Count);

        // Берём подключение у EF Core DbContext, чтобы уважать транзакции/DI и т.д.
        // _db — твой DbContext (например UserMediaContext).
        var db = _db; // предполагается поле в твоём существующем репозитории
        var conn = (NpgsqlConnection)db.Database.GetDbConnection();

        await EnsureOpenAsync(conn, ct).ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand(sql, conn);

        // Если DbContext уже в транзакции — подключим её.
        var currentTx = db.Database.CurrentTransaction;
        if (currentTx is not null)
            cmd.Transaction = (NpgsqlTransaction)currentTx.GetDbTransaction();

        // Параметры
        int i = 0;
        foreach (var r in rows)
        {
            Add(cmd, "id", i, r.Id);
            Add(cmd, "provider", i, r.Provider);
            Add(cmd, "drive", i, r.DriveIdNorm ?? "");
            Add(cmd, "remote", i, r.RemoteItemId);
            Add(cmd, "kind", i, r.Kind);
            Add(cmd, "name", i, r.Name);
            Add(cmd, "path", i, r.Path);
            Add(cmd, "size", i, r.SizeBytes);
            Add(cmd, "mime", i, r.MimeType);
            Add(cmd, "created", i, r.CreatedAt);
            Add(cmd, "modified", i, r.ModifiedAt);
            Add(cmd, "taken", i, r.TakenAt);
            Add(cmd, "w", i, r.Width);
            Add(cmd, "h", i, r.Height);
            Add(cmd, "dur", i, r.DurationSeconds);
            Add(cmd, "halgo", i, r.HashAlgo);
            Add(cmd, "hval", i, r.HashValue);
            Add(cmd, "del", i, r.IsDeleted);
            i++;
        }

        // ExecuteNonQuery вернёт количество "затронутых" строк (insert+update) суммарно.
        return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task EnsureOpenAsync(NpgsqlConnection conn, CancellationToken ct)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct).ConfigureAwait(false);
    }

    private static void Add(NpgsqlCommand cmd, string name, int index, object? value)
    {
        cmd.Parameters.AddWithValue($"{name}_{index}", value ?? DBNull.Value);
    }

    private static string BuildUpsertSql(int n)
    {
        // INSERT INTO media_items (...) VALUES (...),(...),...
        // ON CONFLICT (provider, drive_id_norm, remote_item_id) DO UPDATE SET ...
        var sb = new StringBuilder();
        sb.AppendLine("INSERT INTO media_items (");
        sb.AppendLine("  id, provider, drive_id_norm, remote_item_id, kind, name, path, size_bytes, mime_type,");
        sb.AppendLine("  created_at, modified_at, taken_at, width, height, duration_seconds, hash_algo, hash_value, is_deleted");
        sb.AppendLine(") VALUES");

        for (int i = 0; i < n; i++)
        {
            if (i > 0) sb.AppendLine(",");
            sb.Append(" (");
            sb.Append($"@id_{i}, @provider_{i}, @drive_{i}, @remote_{i}, @kind_{i}, @name_{i}, @path_{i}, @size_{i}, @mime_{i}, ");
            sb.Append($"@created_{i}, @modified_{i}, @taken_{i}, @w_{i}, @h_{i}, @dur_{i}, @halgo_{i}, @hval_{i}, @del_{i}");
            sb.Append(")");
        }

        sb.AppendLine();
        sb.AppendLine("ON CONFLICT (provider, drive_id_norm, remote_item_id) DO UPDATE SET");
        sb.AppendLine("  kind = EXCLUDED.kind,");
        sb.AppendLine("  name = EXCLUDED.name,");
        sb.AppendLine("  path = EXCLUDED.path,");
        sb.AppendLine("  size_bytes = EXCLUDED.size_bytes,");
        sb.AppendLine("  mime_type = EXCLUDED.mime_type,");
        sb.AppendLine("  created_at = EXCLUDED.created_at,");
        sb.AppendLine("  modified_at = EXCLUDED.modified_at,");
        sb.AppendLine("  taken_at = EXCLUDED.taken_at,");
        sb.AppendLine("  width = EXCLUDED.width,");
        sb.AppendLine("  height = EXCLUDED.height,");
        sb.AppendLine("  duration_seconds = EXCLUDED.duration_seconds,");
        sb.AppendLine("  hash_algo = EXCLUDED.hash_algo,");
        sb.AppendLine("  hash_value = EXCLUDED.hash_value,");
        sb.AppendLine("  is_deleted = EXCLUDED.is_deleted;");

        return sb.ToString();
    }
}
