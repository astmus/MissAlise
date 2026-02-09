
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

namespace MissAlise.TelegramBot.Building;

public sealed partial class BotBuilder : IBotBuilder
{
	private readonly BotCommandDescription _root;
	private readonly Stack<BotCommandDescription> _commands = new();
	private BotCommandDescription _current => _commands.Peek();

	public BotBuilder()
	{
		_root = new() { Name = "" };
		_commands.Push(_root);
	}

	public IBotBuilder AddCommand<T>(string name, string? description = null)
		where T : class
	{
		var cmd = CreateCommand(typeof(T), name, description);
		_current.SubCommands.Add(cmd);
		return this;
	}

	public IBotSectionBuilder<T> BeginScope<T>(string name, string description)
	{
		var scope = new BotCommandDescription { Name = name, Description = description };
		_current.SubCommands.Add(scope);
		_commands.Push(scope);
		return new BotSectionBuilder<T>(default, this);
	}

	public IBotBuilder EndSection()
	{
		if (_commands.Count == 0)
			throw new InvalidOperationException("EndSection() called but there is no open section. Check BeginSection/EndSection");

		_commands.Pop();
		return this;
	}

	public BotDefinition Build()
	{
		// на момент Build все открытые секции должны быть закрыты
		//полка  закоменчено потом поправим
		//if (_commands.Count != 1)
		//	throw new InvalidOperationException("EndSection() called but there is no open section. Check BeginSection/EndSection");

		var leafMap = new Dictionary<Command, BotDefinition.LeafRuntime>();
		var leafByPath = new Dictionary<string, BotCommandDescription>(StringComparer.OrdinalIgnoreCase);
		var rootCmd = new RootCommand("MissAlise");

		EnsureHelpCommand();

		//foreach (var c in _root.SubCommands)
		//	rootCmd.AddCommand(BuildCommand(c, leafMap, leafByPath, null));

		BuildCommandRecursive(rootCmd, _root, leafMap, leafByPath, null);

		return new BotDefinition(rootCmd, _root, leafMap, leafByPath);
	}
	private Command BuildCommandRecursive(
					RootCommand root,
					BotCommandDescription d,
					Dictionary<Command, BotDefinition.LeafRuntime> leafMap,
					Dictionary<string, BotCommandDescription> leafByPath,
					string? parentPath)
	{
		Command cmd = root ?? new Command(d.Name, d.Description);
		cmd.AddAlias("/" + d.Name);

		var optMap = new Dictionary<string, Option>(StringComparer.OrdinalIgnoreCase);
		foreach (var p in d.Parameters)
		{
			var opt = CreateOption(p);
			cmd.AddOption(opt);
			optMap[p.Name] = opt;
		}

		var path = parentPath is null ? d.Name : parentPath + " " + d.Name;

		if (d.CommandType is not null)
		{
			leafMap[cmd] = new BotDefinition.LeafRuntime
			{
				Description = d,
				Command = cmd,
				Path = path,
				OptionsByParamKey = optMap
			};

			leafByPath[BotDefinition.NormalizePath(path)] = d;
		}

		foreach (var s in d.GetAllSubCommands())
			cmd.AddCommand(BuildCommandRecursive(null, s, leafMap, leafByPath, path));

		return cmd;
	}

	private void EnsureHelpCommand()
	{
		if (_root.SubCommands.Any(c => c.Name.Equals("help", StringComparison.OrdinalIgnoreCase)))
			return;

		var helpCommand = CreateCommand(typeof(HelpCommand), "help", "Показать справку по командам");
		_root.SubCommands.Add(helpCommand);
	}

	private BotCommandDescription CreateCommand(Type type, string name, string? description)
	{
		var desc = new BotCommandDescription
		{
			Name = name,
			Description = description,
			CommandType = type
		};

		foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!p.CanRead) continue;

			var isNullable = Nullable.GetUnderlyingType(p.PropertyType) != null;

			var parameter = new BotParameterDescription(
				p.Name,
				"--" + ToKebab(p.Name),
				p.PropertyType,
				!isNullable && p.PropertyType != typeof(bool),
				true,
				null,
				p.PropertyType.IsEnum ? Enum.GetNames(p.PropertyType) : null,
				null
			);

			desc.Parameters.Add(parameter);
		}

		return desc;
	}

	private static Option CreateOption(BotParameterDescription p)
	{
		var coreType = Nullable.GetUnderlyingType(p.ValueType) ?? p.ValueType;

		if (coreType == typeof(bool))
			return new Option<bool>(p.CliName, p.Description) { IsRequired = p.IsRequired };

		var opt = (Option)Activator.CreateInstance(
			typeof(Option<>).MakeGenericType(coreType),
			new object?[] { p.CliName, p.Description })!;

		opt.IsRequired = p.IsRequired;
		return opt;
	}

	private static string ToKebab(string s)
		=> string.Concat(s.Select((c, i) =>
			i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
}