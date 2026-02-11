namespace MissAlise.Workflow;

public sealed class WorkflowRequest
{
	public required WorkflowContext Context { get; init; }
	public required WorkflowInput Input { get; init; }
	public WorkflowHandleResult Result { get; internal set; }
	public WorkflowSession Session { get; internal set; }
}
