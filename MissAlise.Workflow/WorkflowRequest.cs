namespace MissAlise.Workflow;

public sealed class WorkflowRequest
{
    public required WorkflowContext Context { get; init; }
    public required WorkflowInput Input { get; init; }
}
