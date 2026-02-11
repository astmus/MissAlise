using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissAlise.TelegramBot.Commands;
/// <summary>
/// Атрибут для связывания record-команды с CLI route.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class BotCommandAttribute : Attribute
{
	public BotCommandAttribute(string route) => Route = route;
	public string Route { get; }
}

// ============================================================================
// OneDrive
// ============================================================================

[BotCommand("onedrive")]
public record OneDriveRoot;

[BotCommand("onedrive sync")]
public record OneDriveSync(Guid? libraryId, bool force) : OneDriveRoot;

[BotCommand("onedrive sync full")]
public sealed record OneDriveSyncFull(Guid? libraryId, bool force) : OneDriveSync(libraryId, force);

[BotCommand("onedrive sync diff")]
public sealed record OneDriveSyncDiff(Guid? libraryId, bool force) : OneDriveRoot;

[BotCommand("onedrive status")]
public sealed record OneDriveStatus : OneDriveRoot;

[BotCommand("onedrive delta")]
public sealed record OneDriveDelta(Guid? libraryId, string? since);

[BotCommand("onedrive auth")]
public sealed record OneDriveAuth;

[BotCommand("onedrive search")]
public sealed record OneDriveSearch(string query, Guid? libraryId, int limit);

public enum MediaKind
{
	Photo,
	Video,
	Audio
}

[BotCommand("media")]
public record MediaRoot;

[BotCommand("media list")]
public sealed record MediaList(MediaKind? kind, int limit) : MediaRoot;

[BotCommand("media info")]
public sealed record MediaInfo(string id) : MediaRoot;

[BotCommand("media delete")]
public sealed record MediaDelete(string id, bool hard) : MediaRoot;

[BotCommand("media search")]
public sealed record MediaSearch(string query, MediaKind? kind, int limit) : MediaRoot;

[BotCommand("media search hash")]
public sealed record MediaSearchByHash(string hash) : MediaRoot;

[BotCommand("media search duplicates")]
public sealed record MediaSearchDuplicates(MediaKind? kind) : MediaRoot;

// ============================================================================
// Jobs
// ============================================================================

[BotCommand("jobs")]
public sealed record JobsRoot;

[BotCommand("jobs list")]
public sealed record JobsList(int limit);

[BotCommand("jobs show")]
public sealed record JobsShow(string id);

[BotCommand("jobs cancel")]
public sealed record JobsCancel(string id);

[BotCommand("jobs retry")]
public sealed record JobsRetry(string id);

[BotCommand("jobs search")]
public sealed record JobsSearch(string query, string? status, int limit);

// ============================================================================
// Settings
// ============================================================================

[BotCommand("settings")]
public sealed record SettingsRoot;

[BotCommand("settings get")]
public sealed record SettingsGet(string key);

[BotCommand("settings set")]
public sealed record SettingsSet(string key, string value);

[BotCommand("settings search")]
public sealed record SettingsSearch(string query);

// ============================================================================
// Global Search (удобно для главного меню)
// ============================================================================

public enum SearchDomain
{
	All,
	Media,
	Jobs,
	OneDrive,
	Reports,
	Settings
}

[BotCommand("search")]
public sealed record GlobalSearch(
	string query,
	SearchDomain domain,
	int limit
);
