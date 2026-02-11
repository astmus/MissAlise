namespace MissAlise.Workflow.Descriptors;

public sealed class WorkflowDescriptor
{
    public required WorkflowId Id { get; init; }
    public required WorkflowStepId StartStep { get; init; }
    public required IReadOnlyDictionary<WorkflowStepId, Type> Steps { get; init; }
}
