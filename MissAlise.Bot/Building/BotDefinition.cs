
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;

namespace MissAlise.TelegramBot.Building;

public class BotCommandAlias : Command
{
	public BotCommandAlias(Type commandType, string name, string? description = null, string? title = null) : base(name, description)
	{
		CommandType = commandType;
		Title = title;
	}

	public string? Title { get; }
	public Type CommandType { get; }
}

public class BotCommand<T> : BotCommandAlias
{
	public BotCommand(string name, string? description = null, string? title = null) : base(typeof(T), name, description, title)
	{
	}

}


public sealed class BotDefinition
{
	public RootCommand RootCommand { get; }
	public BotCommandDescription Description { get; }

	private readonly Dictionary<Command, BotCommandDescription> _leafByCommand;
	private readonly Dictionary<string, BotCommandDescription> _leafByPath;

	internal BotDefinition(RootCommand root, BotCommandDescription tree, Dictionary<Command, BotCommandDescription> leafByCommand, Dictionary<string, BotCommandDescription> leafByPath)
	{
		RootCommand = root;
		Description = tree;
		_leafByCommand = leafByCommand;
		_leafByPath = leafByPath;
	}

	public bool TryResolveLeaf(ParseResult parseResult, out BotCommandDescription leaf)
		=> _leafByCommand.TryGetValue(parseResult.CommandResult.Command, out leaf!);

	public bool TryGetLeafByPath(string path, out BotCommandDescription leaf)
		=> _leafByPath.TryGetValue(NormalizePath(RootCommand.Aliases.First() + " " + path), out leaf!);

	public bool TryGetCommandByPath(string path, out BotCommandDescription? command)
	{
		command = null;
		var normalized = NormalizePath(path);
		if (string.IsNullOrEmpty(normalized)) return false;

		var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		BotCommandDescription? current = Description;

		foreach (var part in parts)
		{
			var next = current?.SubCommands
				.FirstOrDefault(c => c.Name.Equals(part, StringComparison.OrdinalIgnoreCase));

			if (next is null)
				return false;

			current = next;
		}

		command = current;
		return command is not null;
	}

	public IEnumerable<BotCommandDescription> SearchLeafCommands(string query)
	{
		query ??= string.Empty;
		var q = query.Trim();
		if (q.Length == 0)
			return _leafByPath.Values.OrderBy(x => x.Name);

		return _leafByPath
			.Where(kv => kv.Key.Contains(q, StringComparison.OrdinalIgnoreCase) || kv.Value.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
			.Select(kv => kv.Value)
			.Distinct()
			.OrderBy(x => x.Name);
	}

	public sealed record MissingParam(BotParameterDescription Param, string Reason);

	public IReadOnlyList<MissingParam> GetMissing(ParseResult parseResult, BotCommandDescription leaf)
	{
		var missing = new List<MissingParam>();

		foreach (var p in leaf.Parameters.Where(x => x.IsRequired))
		{
			if (!leaf.OptionsByParamKey.TryGetValue(p.Name, out var opt))
				continue;

			var optRes = parseResult.CommandResult.FindResultFor(opt);
			if (optRes is null || optRes.Tokens.Count == 0)
				missing.Add(new MissingParam(p, "Required option not provided"));
		}

		return missing;
	}

	public object BindModel(ParseResult parseResult, BotCommandDescription leaf)
	{
		var t = leaf.CommandType
			?? throw new InvalidOperationException("Leaf has no CommandType");

		var args = leaf.Parameters.Select(p =>
		{
			if (!leaf.OptionsByParamKey.TryGetValue(p.Name, out var opt))
				return default;

			var optRes = parseResult.CommandResult.FindResultFor(opt);
			if (optRes is null)
				return default;

			var value = GetValueForOptionUntyped(parseResult, opt);
			return value;
		}).ToArray();

		var obj = Activator.CreateInstance(t, args)
			?? throw new InvalidOperationException($"Cannot create instance of {t.FullName}");

		return obj;
	}

	private static object? GetValueForOptionUntyped(ParseResult parseResult, Option opt)
	{
		var optType = opt.GetType();
		var argType = optType.GetGenericArguments()[0];

		var mi = typeof(ParseResult)
			.GetMethods()
			.First(m => m.Name == "GetValueForOption" && m.IsGenericMethod)
			.MakeGenericMethod(argType);

		return mi.Invoke(parseResult, new object[] { opt });
	}

	public static string NormalizePath(string path)
	{
		path = (path ?? string.Empty).Trim();
		if (path.StartsWith('/')) path = path[1..];
		return string.Join(' ', path.Split(' ', StringSplitOptions.RemoveEmptyEntries));
	}
}
