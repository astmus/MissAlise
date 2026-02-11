using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Presentation;

namespace MissAlise.Workflow;

public class WorkflowHandleResult
{
	public required WorkflowPresentation Presentation { get; init; }
	public IReadOnlyList<object> ProducedResults { get; init; } = Array.Empty<object>();
	public WorkflowContext Context { get; init; }

	public static WorkflowHandleResult Expired()
		=> new WorkflowExpiredHandleResult()
		{
			Presentation = new WorkflowPresentation() { Text = "⏳ Сессия истекла.\n\nНачните заново.", },			
		};
}

file class WorkflowExpiredHandleResult : WorkflowHandleResult
{	
	public bool IsExpired { get; } = true;
}
