using MissAlise.Workflow.Descriptors;

namespace MissAlise.Workflow;

public sealed class WorkflowCoordinatorOptions
{
	/// <summary>Workflow IDs for which the session is deleted after producing commands (e.g. CLI parameter collection).</summary>
	public HashSet<WorkflowId> DisposeSessionAfterProduce { get; } = new();
}
