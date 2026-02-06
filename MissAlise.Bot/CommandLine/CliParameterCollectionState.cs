namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Keys used in WorkflowSession.State for CLI parameter collection workflow.</summary>
public static class CliParameterCollectionState
{
	public const string CommandPath = "cli:cmd";
	public const string MissingList = "cli:missing";
	public const string CurrentIndex = "cli:idx";
	public const string ValuePrefix = "cli:val:";
}
