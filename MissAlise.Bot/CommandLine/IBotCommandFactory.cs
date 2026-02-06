using MissAlise.Workflow;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Builds an application command (ICommand) from CLI command path and collected parameter values for IWorkflowCommandSink.</summary>
public interface IBotCommandFactory
{
	/// <summary>Creates command instance from command path and collected values and context (e.g. UserId). Returns null if command is not registered.</summary>
	object? CreateCommand(string commandPath, IReadOnlyDictionary<string, string> collectedValues, WorkflowContext context);
}
