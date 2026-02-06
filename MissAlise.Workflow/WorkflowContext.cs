namespace MissAlise.Workflow;

public sealed class WorkflowContext
{
    public required long ChatId { get; init; }
    /// <summary>Telegram user id (or other provider id).</summary>
    public required long UserId { get; init; }
    /// <summary>Resolved internal owner id (e.g. Guid), set by the handler when available.</summary>
    public Guid? ResolvedOwnerId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Locale { get; init; }
}
