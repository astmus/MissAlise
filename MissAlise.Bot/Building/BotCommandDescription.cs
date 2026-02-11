using System;
using System.Collections.Generic;
using System.CommandLine;

namespace MissAlise.TelegramBot.Building;

public sealed class BotCommandDescription
{
	public required string Name { get; init; }
	public string? Description { get; init; }
	public Type? CommandType { get; init; }

	public BotCommandAlias Command { get; init; }
	public string Path { get; init; }

	public IReadOnlyDictionary<string, Option> OptionsByParamKey { get; init; }
	public IReadOnlyList<BotCommandDescription> SubCommands { get; init; }
	public IReadOnlyList<BotParameterDescription> Parameters { get; init; }
}
