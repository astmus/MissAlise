
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;

namespace MissAlise.TelegramBot.Building;

public sealed class BotDefinition
{
	public RootCommand RootCommand { get; }
	public BotCommandDescription DescriptionTree { get; }

	private readonly Dictionary<Command, LeafRuntime> _leafByCommand;
	private readonly Dictionary<string, BotCommandDescription> _leafByPath;

	internal BotDefinition(RootCommand root, BotCommandDescription tree, Dictionary<Command, LeafRuntime> leafByCommand, Dictionary<string, BotCommandDescription> leafByPath)
	{
		RootCommand = root;
		DescriptionTree = tree;
		_leafByCommand = leafByCommand;
		_leafByPath = leafByPath;
	}

	public bool TryResolveLeaf(ParseResult parseResult, out LeafRuntime leaf)
		=> _leafByCommand.TryGetValue(parseResult.CommandResult.Command, out leaf!);

	public bool TryGetLeafByPath(string path, out BotCommandDescription leaf)
			=> _leafByPath.TryGetValue(NormalizePath(path), out leaf!);

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

	public sealed class LeafRuntime
	{
		public required BotCommandDescription Description { get; init; }
		public required Command Command { get; init; }
		public required string Path { get; init; }
		public required IReadOnlyDictionary<string, Option> OptionsByParamKey { get; init; }
	}

	public sealed record MissingParam(BotParameterDescription Param, string Reason);

	public IReadOnlyList<MissingParam> GetMissing(ParseResult parseResult, LeafRuntime leaf)
	{
		var missing = new List<MissingParam>();

		foreach (var p in leaf.Description.Parameters.Where(x => x.IsRequired))
		{
			if (!leaf.OptionsByParamKey.TryGetValue(p.Name, out var opt))
				continue;

			var optRes = parseResult.CommandResult.FindResultFor(opt);
			if (optRes is null || optRes.Tokens.Count == 0)
				missing.Add(new MissingParam(p, "Required option not provided"));
		}

		return missing;
	}

	public object BindModel(ParseResult parseResult, LeafRuntime leaf)
	{
		var t = leaf.Description.CommandType
			?? throw new InvalidOperationException("Leaf has no CommandType");

		var obj = Activator.CreateInstance(t)
			?? throw new InvalidOperationException($"Cannot create instance of {t.FullName}");

		foreach (var p in leaf.Description.Parameters)
		{
			if (!leaf.OptionsByParamKey.TryGetValue(p.Name, out var opt))
				continue;

			var optRes = parseResult.CommandResult.FindResultFor(opt);
			if (optRes is null)
				continue;

			var value = GetValueForOptionUntyped(parseResult, opt);
			var prop = t.GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance);

			if (prop?.CanWrite == true)
				prop.SetValue(obj, value);
		}

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
