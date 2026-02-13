
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MissAlise.TelegramBot.Building.Attributes;
using MissAlise.TelegramBot.Building.Values;
using MissAlise.Workflow.Demo.BackgroundSync;

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

				var chooseAttr = arg.GetCustomAttribute<BotValueChooseAttribute>();
				var rangeAttr = arg.GetCustomAttribute<BotRangeAttribute>();
				var stepAttr = arg.GetCustomAttribute<BotStepAttribute>();
				var boolGroupAttr = arg.GetCustomAttribute<BotBoolStateGroupAttribute>();
				var spinTimeAttr = arg.GetCustomAttribute<BotValueSpinTimeAttribute>();

				// Собираем все настройки параметра в один объект Value<T>/NumericValue<T>/StepValue<T>.
				// Этот объект кладём в BotParameterDescription.DefaultValue.
				var valueMeta = BuildValueMeta(arg.ParameterType, chooseAttr, rangeAttr, stepAttr, boolGroupAttr, spinTimeAttr);

				var parameter = new BotParameterDescription(
					arg.Name!,
					"--" + ToKebab(arg.Name!),
					valueType,
					!isNullable,
					true,
					descriptionAttribute?.Description,
					null,
					valueMeta
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

	private static ValueBase? BuildValueMeta(
		Type valueType,
		BotValueChooseAttribute? chooseAttr,
		BotRangeAttribute? rangeAttr,
		BotStepAttribute? stepAttr,
		BotBoolStateGroupAttribute? boolGroupAttr,
		BotValueSpinTimeAttribute? spinTimeAttr)
	{
		var coreType = Nullable.GetUnderlyingType(valueType) ?? valueType;

		// 0) Bool -> BoolValue + visual group
		if (coreType == typeof(bool))
		{
			return new BoolValue(chooseAttr?.DefaultValue is bool db ? db : default(bool?))
			{
				VisualGroupKey = boolGroupAttr?.GroupKey,
				AllowedValues = chooseAttr?.Values?.OfType<bool>().ToArray(),
			};
		}

		// 1) Специальный кейс: TimeSpan spin (у тебя уже есть такая модель в BackgroundSyncCommand)
		if (coreType == typeof(TimeSpan) && spinTimeAttr is not null)
		{
			// TimeSpan НЕ реализует generic-math IAdditionOperators/ISubtractionOperators,
			// поэтому используем StepValue<TimeSpan> с делегатами.
			return ValueFactory.TimeSpanSpinner(
				@default: spinTimeAttr.Value,
				min: spinTimeAttr.Min,
				max: spinTimeAttr.Max,
				step: spinTimeAttr.Diff,
				multipliers: new[] { 1 },
				formatter: static ts => ts.ToString());
		}

		// 2) Enum -> AllowedValues = Enum names
		if (coreType.IsEnum)
		{
			var names = Enum.GetNames(coreType);
			return new Value<string>(default) { AllowedValues = names };
		}

		// 3) Общий кейс: NumericValue<T> для числовых типов (int/long/double/decimal)
		// Мы создаём NumericValue<T> только для типов, которые поддерживают INumber<T>.
		// В противном случае — просто Value<T>.
		if (TryBuildNumericValue(coreType, chooseAttr, rangeAttr, stepAttr, out var numeric))
			return numeric;

		// 4) Choice (нечисловой) -> Value<T> + AllowedValues
		if (chooseAttr is not null)
		{
			return new Value<object>(chooseAttr.DefaultValue)
			{
				AllowedValues = chooseAttr.Values
			};
		}

		// 5) Просто default
		return null;
	}

	private static bool ImplementsGenericInterface(Type type, Type genericInterfaceDefinition)
	{
		if (!genericInterfaceDefinition.IsGenericTypeDefinition)
			return false;
		foreach (var iface in type.GetInterfaces())
		{
			if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericInterfaceDefinition)
			{
				var args = iface.GetGenericArguments();
				if (args.Length == 1 && args[0] == type)
					return true;
			}
		}
		return false;
	}

	private static bool TryBuildNumericValue(
		Type coreType,
		BotValueChooseAttribute? chooseAttr,
		BotRangeAttribute? rangeAttr,
		BotStepAttribute? stepAttr,
		out ValueBase? value)
	{
		value = null;
		if (!coreType.IsValueType)
			return false;
		if (!ImplementsGenericInterface(coreType, typeof(System.Numerics.INumber<>)))
			return false;
		if (!ImplementsGenericInterface(coreType, typeof(IParsable<>)))
			return false;

		var method = typeof(BotBuilder).GetMethod(nameof(CreateNumericValue), BindingFlags.NonPublic | BindingFlags.Static)!
			.MakeGenericMethod(coreType);
		value = method.Invoke(null, new object?[] { chooseAttr, rangeAttr, stepAttr }) as ValueBase;
		return value != null;
	}

	private static NumericValue<T> CreateNumericValue<T>(
		BotValueChooseAttribute? chooseAttr,
		BotRangeAttribute? rangeAttr,
		BotStepAttribute? stepAttr)
		where T : struct, System.Numerics.INumber<T>, IParsable<T>
	{
		var defaultVal = chooseAttr?.DefaultValue is T d ? d : default(T?);
		return new NumericValue<T>(defaultVal)
		{
			AllowedValues = chooseAttr?.Values?.OfType<T>().ToArray(),
			Min = TryConvert<T>(rangeAttr?.Min),
			Max = TryConvert<T>(rangeAttr?.Max),
			StepBase = TryConvert<T>(stepAttr?.BaseStep) ?? T.One,
			Multipliers = stepAttr?.Multipliers ?? Array.Empty<int>(),
		};
	}

	private static T? TryConvert<T>(object? value) where T : struct
	{
		if (value is null) return null;
		if (value is T t) return t;
		try
		{
			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}
		catch
		{
			return null;
		}
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