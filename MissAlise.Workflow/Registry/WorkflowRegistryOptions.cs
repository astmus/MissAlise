namespace MissAlise.Workflow.Registry;

public sealed class WorkflowRegistryOptions
{
    public Action<WorkflowRegistry>? Register { get; set; }
}
