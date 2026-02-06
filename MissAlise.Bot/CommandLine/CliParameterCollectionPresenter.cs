using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Presents the current parameter prompt for CLI parameter collection (description from command/help).</summary>
public sealed class CliParameterCollectionPresenter : IWorkflowPresenter
{
	private readonly IBotCommandLineParser _parser;

	public CliParameterCollectionPresenter(IBotCommandLineParser parser)
	{
		_parser = parser ?? throw new ArgumentNullException(nameof(parser));
	}

	public WorkflowPresentation Present(WorkflowSession session, WorkflowContext context)
	{
		var commandPath = session.Get(CliParameterCollectionState.CommandPath);
		var missingStr = session.Get(CliParameterCollectionState.MissingList);
		if (string.IsNullOrEmpty(commandPath) || string.IsNullOrEmpty(missingStr))
			return new WorkflowPresentation { Text = "Сессия ввода параметров. Введите значение или отмените." };

		var missing = missingStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
		var idx = session.GetInt(CliParameterCollectionState.CurrentIndex, 0);
		if (idx < 0 || idx >= missing.Count)
			return new WorkflowPresentation { Text = "Все параметры введены. Команда будет выполнена." };

		var param = missing[idx];
		var description = _parser.GetParameterDescription(commandPath, param) ?? param;
		var text = $"Команда: **{commandPath}**\n\nВведите значение для параметра **{param}**:\n_{description}_";
		return new WorkflowPresentation { Text = text, Buttons = Array.Empty<WorkflowButton>() };
	}
}
