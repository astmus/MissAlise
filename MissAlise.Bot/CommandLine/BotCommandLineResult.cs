namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Result of parsing user input (message or callback) against the CLI definition.</summary>
public abstract record BotCommandLineResult;

/// <summary>All required parameters are present; command can be built via IBotCommandFactory and invoked via IWorkflowCommandSink.</summary>
public sealed record ReadyToInvokeResult(string CommandPath, IReadOnlyDictionary<string, string> CollectedValues) : BotCommandLineResult;

/// <summary>Command identified but one or more parameters are missing; start parameter collection session.</summary>
public sealed record NeedParametersResult(
	string CommandPath,
	IReadOnlyList<ParameterPrompt> MissingParameters,
	IReadOnlyDictionary<string, string> Collected) : BotCommandLineResult;

/// <summary>Description for one parameter (option or argument) to show in prompts.</summary>
public sealed record ParameterPrompt(string SymbolName, string Description, string? HelpName = null);
