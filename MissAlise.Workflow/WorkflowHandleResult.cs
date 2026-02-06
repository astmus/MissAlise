using MissAlise.Workflow.Presentation;

namespace MissAlise.Workflow;

public sealed class WorkflowHandleResult
{
    public required WorkflowPresentation Presentation { get; init; }
    public IReadOnlyList<object> ProducedCommands { get; init; } = Array.Empty<object>();
}
