namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Parses bot input (text or callback payload) against a CLI definition. First-level = bot command, second-level = subcommand (callback), search = inline.</summary>
public interface IBotCommandLineParser
{
	/// <summary>Parse input as a top-level bot command (e.g. message text "/sync" or "sync").</summary>
	BotCommandLineResult? ParseAsBotCommand(string input);

	/// <summary>Parse input as a callback (e.g. "cmd:sync" or "cmd:sync:start" for second-level).</summary>
	BotCommandLineResult? ParseAsCallback(string callbackData);

	/// <summary>Parse input as inline query (search). Returns suggestions or command path for execution.</summary>
	BotCommandLineResult? ParseAsInlineQuery(string query);

	/// <summary>Get description for a parameter (from Option/Argument Description or Help).</summary>
	string? GetParameterDescription(string commandPath, string symbolName);

	/// <summary>Get all first-level command names for registering bot commands.</summary>
	IReadOnlyList<BotCommandInfo> GetTopLevelCommands();
}

public sealed record BotCommandInfo(string Name, string Description);
