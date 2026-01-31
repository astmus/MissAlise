namespace MissAlise.ValueObjects.Media;

public sealed record MediaTimestamps(
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    DateTimeOffset? TakenAt = null
);
