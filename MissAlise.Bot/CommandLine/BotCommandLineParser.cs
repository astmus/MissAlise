using System.Text;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Parses bot input using a System.CommandLine root and maps to bot commands / callbacks / inline.</summary>
public sealed class BotCommandLineParser : IBotCommandLineParser
{
	private const string CallbackPrefix = "cmd:";
	private readonly RootCommand _root;
	private readonly Dictionary<string, string> _paramDescriptions = new(StringComparer.OrdinalIgnoreCase);

	public BotCommandLineParser(RootCommand root)
	{
		_root = root ?? throw new ArgumentNullException(nameof(root));
		IndexDescriptions(_root, "");
	}

	private void IndexDescriptions(Command command, string pathPrefix)
	{
		foreach (var option in command.Options)
		{
			var key = (pathPrefix + "::" + option.Name).TrimStart(':');
			if (!string.IsNullOrWhiteSpace(option.Description))
				_paramDescriptions[key] = option.Description;
		}
		foreach (var arg in command.Arguments)
		{
			var key = (pathPrefix + "::" + arg.Name).TrimStart(':');
			if (!string.IsNullOrWhiteSpace(arg.Description))
				_paramDescriptions[key] = arg.Description;
		}
		foreach (var sub in command.Subcommands)
		{
			var subPath = string.IsNullOrEmpty(pathPrefix) ? sub.Name : pathPrefix + " " + sub.Name;
			IndexDescriptions(sub, subPath);
		}
	}

	public BotCommandLineResult? ParseAsBotCommand(string input)
	{
		if (string.IsNullOrWhiteSpace(input)) return null;
		var args = Tokenize(TrimSlash(input));
		return Parse(args);
	}

	public BotCommandLineResult? ParseAsCallback(string callbackData)
	{
		if (string.IsNullOrWhiteSpace(callbackData) || !callbackData.StartsWith(CallbackPrefix, StringComparison.OrdinalIgnoreCase))
			return null;
		var rest = callbackData[CallbackPrefix.Length..].Trim();
		var args = Tokenize(rest);
		return Parse(args);
	}

	public BotCommandLineResult? ParseAsInlineQuery(string query)
	{
		if (string.IsNullOrWhiteSpace(query)) return null;
		var args = Tokenize(query.Trim());
		return Parse(args);
	}

	public string? GetParameterDescription(string commandPath, string symbolName)
	{
		var key = commandPath + "::" + symbolName;
		return _paramDescriptions.TryGetValue(key, out var d) ? d : _paramDescriptions.GetValueOrDefault(symbolName);
	}

	public IReadOnlyList<BotCommandInfo> GetTopLevelCommands()
	{
		return _root.Subcommands
			.Select(c => new BotCommandInfo(c.Name, c.Description ?? ""))
			.ToList();
	}

	private BotCommandLineResult? Parse(string[] args)
	{
		if (args.Length == 0) return null;
		var result = _root.Parse(args);
		if (result.Errors.Count > 0)
			return null;

		var (path, collected, missing) = CollectRequired(result);
		if (missing.Count == 0)
			return new ReadyToInvokeResult(path, new Dictionary<string, string>(collected));

		var prompts = missing.Select(m => new ParameterPrompt(
			m,
			GetParameterDescription(path, m) ?? m,
			null)).ToList();
		return new NeedParametersResult(path, prompts, new Dictionary<string, string>(collected));
	}

	private static (string path, Dictionary<string, string> collected, List<string> missing) CollectRequired(ParseResult result)
	{
		var path = new List<string>();
		var collected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var missing = new List<string>();
		Walk(result.RootCommandResult, result, path, collected, missing);
		return (string.Join(" ", path), collected, missing);
	}

	private static void Walk(CommandResult cmdResult, ParseResult parseResult, List<string> path, Dictionary<string, string> collected, List<string> missing)
	{
		path.Add(cmdResult.Symbol.Name);
		foreach (var opt in cmdResult.Children.OfType<OptionResult>())
		{
			var val = opt.GetValueOrDefault<string>();
			if (val != null)
				collected[opt.Symbol.Name] = val;
			else
				missing.Add(opt.Symbol.Name);
		}
		foreach (var arg in cmdResult.Children.OfType<ArgumentResult>())
		{
			var val = arg.GetValueOrDefault<string>();
			if (val != null)
				collected[arg.Symbol.Name] = val;
			else
				missing.Add(arg.Symbol.Name);
		}
		var sub = cmdResult.Children.OfType<CommandResult>().FirstOrDefault();
		if (sub != null)
			Walk(sub, parseResult, path, collected, missing);
	}

	private static string TrimSlash(string text)
	{
		var t = text.Trim();
		if (t.StartsWith("/", StringComparison.Ordinal))
			t = t.Length > 1 ? t[1..] : "";
		return t;
	}

	private static string[] Tokenize(string input)
	{
		var list = new List<string>();
		var current = new StringBuilder();
		var inQuotes = false;
		for (var i = 0; i < input.Length; i++)
		{
			var c = input[i];
			if (c == '"' || c == '\'')
			{
				inQuotes = !inQuotes;
				continue;
			}
			if (!inQuotes && (char.IsWhiteSpace(c) || c == ':'))
			{
				if (current.Length > 0)
				{
					list.Add(current.ToString());
					current.Clear();
				}
				if (c == ':' && list.Count > 0)
					continue;
			}
			else
				current.Append(c);
		}
		if (current.Length > 0)
			list.Add(current.ToString());
		return list.ToArray();
	}
}
