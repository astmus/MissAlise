
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MissAlise.TelegramBot.Building.Attributes;
using MissAlise.Workflow.Demo.BackgroundSync;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace MissAlise.TelegramBot.Building;

public sealed partial class BotBuilder : IBotBuilder
{
	//private readonly BotCommandDescription _root;
	private readonly Stack<Command> _commands = new();
	private Command _current => _commands.Peek();

	public BotBuilder()
	{
		_commands.Push(new RootCommand("MissAlise"));
	}

	public IBotBuilder AddCommand<T>(string name, string description, string? title = null)
		where T : class
	{
		var cmd = new BotCommand<T>(name, description, title);
		_current.AddCommand(cmd);
		return this;
	}

	public IBotSectionBuilder<T> BeginScope<T>(string name, string description, string? title = null)
	{
		var scope = new BotCommand<T>(name, description, title);
		_current.AddCommand(scope);
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

		var leafMap = new Dictionary<Command, BotCommandDescription>();
		var leafByPath = new Dictionary<string, BotCommandDescription>(StringComparer.OrdinalIgnoreCase);

		EnsureHelpCommand();

		//foreach (var c in _root.SubCommands)
		//	rootCmd.AddCommand(BuildCommand(c, leafMap, leafByPath, null));

		var rootCmd = _commands.Pop() as RootCommand;

		var descRoot = BuildCommandRecursive(rootCmd, leafMap, leafByPath, null);

		return new BotDefinition(rootCmd, descRoot, leafMap, leafByPath);
	}
	private BotCommandDescription BuildCommandRecursive(
					Command current,
					Dictionary<Command, BotCommandDescription> leafMap,
					Dictionary<string, BotCommandDescription> leafByPath,
					string? parentPath)
	{
		ArgumentNullException.ThrowIfNull(current, nameof(current));

		current.AddAlias("/" + current.Name);

		var path = parentPath is null ? current.Name : parentPath + " " + current.Name;
		var commandType = (current as BotCommandAlias)?.CommandType;

		var description = CreateCommandDescription(commandType, current, path, out var subCommands);

		leafMap[current] = description;
		leafByPath[BotDefinition.NormalizePath(path)] = description;

		foreach (var sub in current.Subcommands)
			subCommands.Add(BuildCommandRecursive(sub, leafMap, leafByPath, path));

		return description;
	}

	private void EnsureHelpCommand()
	{
		if (_current.Subcommands.Any(c => c.Name.Equals("help", StringComparison.OrdinalIgnoreCase)))
			return;

		AddCommand<HelpCommand>("help", "Показать справку по командам");
	}

	private static BotCommandDescription CreateCommandDescription(Type commandType, Command aliaseCmd, string path, out List<BotCommandDescription> subCommands)
	{
		List<BotParameterDescription> parameters = null;
		Dictionary<string, Option> options = null;
		subCommands = new List<BotCommandDescription>();

		//var properties = commandType?.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
		var ctor = commandType?.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
		var ctorArgs = ctor?.GetParameters();

		if (ctorArgs?.Any() == true)
		{
			parameters = new List<BotParameterDescription>();
			options = new Dictionary<string, Option>(StringComparer.OrdinalIgnoreCase);

			foreach (var arg in ctorArgs)
			{

				var isNullable = Nullable.GetUnderlyingType(arg.ParameterType) != null;
				var valueType = arg.ParameterType;
				var descriptionAttribute = arg.GetCustomAttribute<BotDescriptionAttribute>();

				var allowedValues = arg.GetCustomAttribute<BotValueChooseAttribute>()?.Values;				
				var provider = arg.GetCustomAttribute<BotValueSpinTimeAttribute>()?.ValueProvider;

				var parameter = new BotParameterDescription(
					arg.Name,
					"--" + ToKebab(arg.Name),
					valueType,
					!isNullable,
					true,
					descriptionAttribute?.Description,
					allowedValues ?? (valueType.IsEnum ? Enum.GetNames(valueType) : null),
					null,
					provider
				);

				var opt = CreateOption(parameter);
				aliaseCmd.AddOption(opt);

				options[arg.Name] = opt;
				parameters.Add(parameter);
			}
		}

		return new BotCommandDescription
		{
			Name = aliaseCmd.Name,
			Description = aliaseCmd.Description,
			Command = aliaseCmd as BotCommandAlias,
			CommandType = commandType,
			Path = path,
			SubCommands = subCommands,
			Parameters = parameters?.AsReadOnly() ?? Array.Empty<BotParameterDescription>().AsReadOnly(),
			OptionsByParamKey = options?.AsReadOnly(),
		};
	}

	private static Option CreateOption(BotParameterDescription arg)
	{
		var coreType = Nullable.GetUnderlyingType(arg.ValueType) ?? arg.ValueType;

		if (coreType == typeof(bool))
			return new Option<bool>(arg.CliName, arg.Description) { IsRequired = arg.IsRequired };

		var opt = (Option)Activator.CreateInstance(
			typeof(Option<>).MakeGenericType(coreType),
			new object?[] { arg.CliName, arg.Description })!;

		opt.IsRequired = arg.IsRequired;
		return opt;
	}

	private static string ToKebab(string s)
		=> string.Concat(s.Select((c, i) =>
			i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
}