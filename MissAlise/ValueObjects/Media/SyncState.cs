namespace MissAlise.ValueObjects.Media;

public enum SyncStatus
{
    Never = 0,
    Success = 1,
    Failed = 2,
    InProgress = 3
}

public sealed record SyncState(
    string? Cursor,
    DateTimeOffset? LastStartedAt,
    DateTimeOffset? LastFinishedAt,
    SyncStatus Status,
    string? LastError
)
{
    public static readonly SyncState Empty = new(
        Cursor: null,
        LastStartedAt: null,
        LastFinishedAt: null,
        Status: SyncStatus.Never,
        LastError: null
    );

    public SyncState Start(DateTimeOffset now)
        => this with { LastStartedAt = now, Status = SyncStatus.InProgress, LastError = null };

    public SyncState Success(DateTimeOffset now, string? newCursor)
        => this with { LastFinishedAt = now, Status = SyncStatus.Success, Cursor = newCursor, LastError = null };

    public SyncState Fail(DateTimeOffset now, string error)
        => this with { LastFinishedAt = now, Status = SyncStatus.Failed, LastError = error };
}
