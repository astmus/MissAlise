namespace MissAlise.Workflow.Presentation;

public sealed class WorkflowButton
{
	public string Group { get; set; }
	public required string Text { get; init; }
	public required string Payload { get; init; }
}
