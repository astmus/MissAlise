using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Delegates to CLI parameter presenter for "cli-params" workflow, otherwise to the default presenter.</summary>
public sealed class CompositeWorkflowPresenter : IWorkflowPresenter
{
	public static readonly string CliParamsWorkflowId = "cli-params";

	private readonly IWorkflowPresenter _cliParamsPresenter;
	private readonly IWorkflowPresenter _defaultPresenter;

	public CompositeWorkflowPresenter(
		IWorkflowPresenter cliParamsPresenter,
		IWorkflowPresenter defaultPresenter)
	{
		_cliParamsPresenter = cliParamsPresenter ?? throw new ArgumentNullException(nameof(cliParamsPresenter));
		_defaultPresenter = defaultPresenter ?? throw new ArgumentNullException(nameof(defaultPresenter));
	}

	public WorkflowPresentation Present(WorkflowSession session, WorkflowContext context)
	{
		return session.WorkflowId.Value == CliParamsWorkflowId
			? _cliParamsPresenter.Present(session, context)
			: _defaultPresenter.Present(session, context);
	}
}
