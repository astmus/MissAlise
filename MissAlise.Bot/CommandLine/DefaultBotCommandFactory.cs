using MissAlise.Application.Commands;
using MissAlise.ValueObjects;
using MissAlise.Workflow;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Creates ICommand instances from CLI command path and collected values. Maps "start" -> StartCommand, "sync" -> SyncCommand, etc.</summary>
public sealed class DefaultBotCommandFactory : IBotCommandFactory
{
	public object? CreateCommand(string commandPath, IReadOnlyDictionary<string, string> collectedValues, WorkflowContext context)
	{
		var path = commandPath.Trim().Replace(" ", ":", StringComparison.Ordinal);
		if (path.IndexOf("start", StringComparison.OrdinalIgnoreCase) > -1)
			return new StartCommand();

		if (path.IndexOf("sync", StringComparison.OrdinalIgnoreCase) > -1)
		{
			var userId = context.ResolvedOwnerId.HasValue
				? new UserId(context.ResolvedOwnerId.Value)
				: default;
			return new SyncCommand(userId);
		}

		return null;
	}
}
