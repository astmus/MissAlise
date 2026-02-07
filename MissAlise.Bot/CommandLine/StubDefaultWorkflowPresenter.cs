using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Fallback when no IDefaultWorkflowPresenter is registered (e.g. only CLI workflow is used).</summary>
public sealed class StubDefaultWorkflowPresenter : IDefaultWorkflowPresenter
{
	public WorkflowPresentation Present(WorkflowSession session, WorkflowContext context)
		=> new WorkflowPresentation { Text = "Команда для обработки не найдена", Buttons = Array.Empty<WorkflowButton>() };
}
