namespace MissAlise.Workflow.Presentation;

public sealed class WorkflowPresentation
{
	public static readonly WorkflowPresentation Empty = new() { Text = "" };
	public required string Text { get; init; }
	public IEnumerable<IEnumerable<WorkflowButton>> Buttons { get; init; } = Array.Empty<List<WorkflowButton>>();
}
