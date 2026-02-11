namespace MissAlise.Workflow;

public sealed class WorkflowContext
{
	public required long ChatId { get; init; }
	public required long UserId { get; init; }
	public string? CorrelationId { get; init; }
	public string? Locale { get; init; }
}
