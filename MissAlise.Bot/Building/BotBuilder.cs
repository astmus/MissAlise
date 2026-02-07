
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;

namespace MissAlise.TelegramBot.Building;

public sealed class BotBuilder
{
	private readonly BotCommandDescription _root = new() { Name = "root" };

	public BotBuilder AddCommand<T>(string name, string? description = null)
		where T : class
	{
		var cmd = CreateCommand(typeof(T), name, description);
		_root.SubCommands.Add(cmd);
		return this;
	}

	public BotBuilder AddSubCommand<T>(BotCommandDescription parent, string name, string? description = null)
		where T : class
	{
		var cmd = CreateCommand(typeof(T), name, description);
		parent.SubCommands.Add(cmd);
		return this;
	}

	public BotDefinition Build()
	{
		var leafMap = new Dictionary<Command, BotDefinition.LeafRuntime>();
		var leafByPath = new Dictionary<string, BotCommandDescription>(StringComparer.OrdinalIgnoreCase);
		var rootCmd = new RootCommand("MissAlise");

		EnsureHelpCommand();

		foreach (var c in _root.SubCommands)
			rootCmd.AddCommand(BuildCommand(c, leafMap, leafByPath, null));

		return new BotDefinition(rootCmd, _root, leafMap, leafByPath);
	}

	private Command BuildCommand(BotCommandDescription d, Dictionary<Command, BotDefinition.LeafRuntime> leafMap, Dictionary<string, BotCommandDescription> leafByPath, string? parentPath)
	{
		var cmd = new Command(d.Name, d.Description);
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

		foreach (var s in d.SubCommands)
			cmd.AddCommand(BuildCommand(s, leafMap, leafByPath, parentPath));

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

			desc.Parameters.Add(new BotParameterDescription(
				p.Name,
				"--" + ToKebab(p.Name),
				p.PropertyType,
				!isNullable && p.PropertyType != typeof(bool),
				true,
				null,
				p.PropertyType.IsEnum ? Enum.GetNames(p.PropertyType) : null,
				null
			));
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
