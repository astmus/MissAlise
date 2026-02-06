using MissAlise.Workflow.Descriptors;

namespace MissAlise.Workflow.Registry;

public sealed class WorkflowRegistry
{
	private readonly Dictionary<WorkflowId, WorkflowDescriptor> _items = new();

	public void Register(WorkflowDescriptor descriptor)
	{
		ArgumentNullException.ThrowIfNull(descriptor);
		_items[descriptor.Id] = descriptor;
	}

	public WorkflowDescriptor Get(WorkflowId id)
		=> 
		_items.TryGetValue(id, out var d) ? d : throw new KeyNotFoundException($"Workflow '{id}' is not registered.");

	public bool TryGet(WorkflowId id, out WorkflowDescriptor descriptor)
		=> _items.TryGetValue(id, out descriptor!);
}
