using MissAlise.Workflow;

namespace MissAlise.TelegramBot.Workflow;

public interface IWorkflowResultRenderer
{
	Task RenderAsync(WorkflowHandleResult result, CancellationToken cancellationToken = default);
}
