namespace MissAlise.Workflow.Presentation;

public sealed class WorkflowPresentation
{
    public required string Text { get; init; }
    public IReadOnlyList<WorkflowButton> Buttons { get; init; } = Array.Empty<WorkflowButton>();
}
