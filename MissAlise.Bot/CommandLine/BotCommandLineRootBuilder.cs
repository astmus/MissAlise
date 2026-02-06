using System.CommandLine;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Builds the System.CommandLine RootCommand used for bot commands (first-level = bot commands, second-level = subcommands for callbacks).</summary>
public static class BotCommandLineRootBuilder
{
	/// <summary>Builds a root with start and sync as first-level commands. Extend as needed.</summary>
	public static RootCommand Build()
	{
		var root = new RootCommand("MissAlise bot commands");
		root.AddCommand(new Command("start", "Start the bot and authorize"));
		var sync = new Command("sync", "Synchronize OneDrive folder");
		sync.AddOption(new Option<string>("--path", "Local path for sync") { IsRequired = false });
		sync.AddOption(new Option<string>("--user", "User identifier") { IsRequired = false });
		root.AddCommand(sync);
		return root;
	}
}
