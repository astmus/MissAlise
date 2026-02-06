using MissAlise.Workflow.Presentation;

namespace MissAlise.TelegramBot.Workflow;

public interface IWorkflowResultRenderer
{
	Task RenderAsync(long chatId, WorkflowPresentation presentation, CancellationToken cancellationToken = default);
}
