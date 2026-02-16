using System;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using MissAlise.TelegramBot.Building.Values;

namespace MissAlise.TelegramBot.Building.Attributes;

/// <summary>
/// Единый атрибут параметра команды: описание, выбор значений, диапазон, шаг, TimeSpan-спиннер, группа bool, валидатор Option.
/// Заменяет BotDescriptionAttribute, BotValueChooseAttribute, BotRangeAttribute, BotStepAttribute, BotValueSpinTimeAttribute, BotBoolStateGroupAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
public sealed class BotParameterAttribute : Attribute
{
	public string Description { get; init; }
	public object? DefaultValue { get; init; }
	public object[]? Values { get; init; }
	public object? Min { get; init; }
	public object? Max { get; init; }
	public object? BaseStep { get; init; }
	public int[]? Multipliers { get; init; } = [1];
	/// <summary>Ключ визуального состояния (общий для всех типов).</summary>
	public string? VisualStateKey { get; init; }
	/// <summary>Строка форматирования текущего значения (например &quot;{0} min&quot;).</summary>
	public string? Format { get; init; }

	public BotParameterAttribute() { }

	/// <summary>Описание и/или ключ визуального состояния (одна строка задаёт оба).</summary>
	public BotParameterAttribute(string description, string? visualStateKey)
	{
		Description = description;
		VisualStateKey = visualStateKey;
	}

	public BotParameterAttribute(object? defaultValue, params object[]? values)
	{
		DefaultValue = defaultValue;
		Values = values;
	}

	public BotParameterAttribute(string description, object? defaultValue, params object[]? values)
	{
		Description = description;
		DefaultValue = defaultValue;
		Values = values;
	}

	/// <summary>Числовой параметр: описание, min, max, baseStep, множители шага.</summary>
	public BotParameterAttribute(string? description, object min, object max, object baseStep, params int[]? multipliers)
	{
		Description = description;
		Min = min;
		Max = max;
		BaseStep = baseStep;
		Multipliers = multipliers;
	}

	/// <summary>TimeSpan-спиннер: default, min, max, step, format (например &quot;{0} min&quot;).</summary>
	public BotParameterAttribute(string value, string min, string max, string diff, string format)
	{
		DefaultValue = value;
		Min = min;
		Max = max;
		BaseStep = diff;
		Format = format;
	}

	/// <summary>Создаёт ValueBase по типу параметра и данным атрибута.</summary>
	public ValueBase? CreateValueMeta(Type parameterType)
	{
		var coreType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

		if (coreType == typeof(bool))
			return CreateBoolValue();
		if (coreType == typeof(TimeSpan) && (DefaultValue is not null || Min is not null || Max is not null || BaseStep is not null))
			return CreateTimeSpanSpinner();
		if (coreType.IsEnum)
			return CreateEnumValue(coreType);
		var numeric = CreateNumericValue(coreType);
		if (numeric is not null)
			return numeric;
		if (DefaultValue is not null || (Values?.Length > 0))
			return CreateChoiceValue();
		return null;
	}

	private BoolValue CreateBoolValue()
	{
		var def = DefaultValue is bool b ? b : default(bool?);
		return new BoolValue(def)
		{
			VisualStateKey = VisualStateKey,
			AllowedValues = Values?.OfType<bool>().ToArray(),
		};
	}

	private StepValue<TimeSpan> CreateTimeSpanSpinner()
	{
		var v = ToTimeSpan(DefaultValue) ?? TimeSpan.Zero;
		var min = ToTimeSpan(Min);
		var max = ToTimeSpan(Max);
		var step = ToTimeSpan(BaseStep) ?? TimeSpan.FromMinutes(1);
		var format = Format;
		return new StepValue<TimeSpan>(v)
		{
			VisualStateKey = VisualStateKey,
			Min = min,
			Max = max,
			StepBase = step,
			Multipliers = Multipliers,
			Add = static (a, b) => a + b,
			Sub = static (a, b) => a - b,
			Formatter = format is not null ? ts => string.Format(CultureInfo.CurrentCulture, format, ts) : null,
		};
	}

	private static TimeSpan? ToTimeSpan(object? o)
	{
		if (o is null) return null;
		if (o is TimeSpan t) return t;
		var s = o.ToString();
		if (string.IsNullOrWhiteSpace(s)) return null;
		return TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
	}

	private ValueBase CreateEnumValue(Type enumType)
	{
		var names = Enum.GetNames(enumType);
		return new Value<string>(default!) { VisualStateKey = VisualStateKey, AllowedValues = names };
	}

	/// <summary>Создаёт NumericValue для типа, реализующего INumber и IParsable. Вызов по coreType без отражения.</summary>
	private ValueBase? CreateNumericValue(Type coreType)
	{
		if (coreType == typeof(int)) return CreateNumericValue<int>(this);
		if (coreType == typeof(long)) return CreateNumericValue<long>(this);
		if (coreType == typeof(double)) return CreateNumericValue<double>(this);
		if (coreType == typeof(decimal)) return CreateNumericValue<decimal>(this);
		return null;
	}

	private static NumericValue<T> CreateNumericValue<T>(BotParameterAttribute attr)
		where T : struct, INumber<T>, IParsable<T>
	{
		var defaultVal = attr.DefaultValue is T d ? d : TryConvert<T>(attr.DefaultValue);
		return new NumericValue<T>(defaultVal ?? default)
		{
			VisualStateKey = attr.VisualStateKey,
			AllowedValues = attr.Values?.OfType<T>().ToArray(),
			Min = TryConvert<T>(attr.Min),
			Max = TryConvert<T>(attr.Max),
			StepBase = TryConvert<T>(attr.BaseStep) ?? T.One,
			Multipliers = attr.Multipliers ?? Array.Empty<int>(),
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

	private ValueBase CreateChoiceValue()
	{
		return new ChoiceValue(DefaultValue)
		{
			VisualStateKey = VisualStateKey,
			AllowedValues = Values,
		};
	}

	/// <summary>Сливает несколько атрибутов с одного параметра в один (первые непустые значения побеждают).</summary>
	public static BotParameterAttribute? Merge(System.Collections.Generic.IEnumerable<BotParameterAttribute> attrs)
	{
		var list = attrs?.ToList();
		if (list is null || list.Count == 0) return null;
		if (list.Count == 1) return list[0];

		return new BotParameterAttribute
		{
			Description = list.FirstOrDefault(a => !string.IsNullOrEmpty(a.Description))?.Description,
			DefaultValue = list.FirstOrDefault(a => a.DefaultValue is not null)?.DefaultValue,
			Values = list.FirstOrDefault(a => a.Values?.Length > 0)?.Values,
			Min = list.FirstOrDefault(a => a.Min is not null)?.Min,
			Max = list.FirstOrDefault(a => a.Max is not null)?.Max,
			BaseStep = list.FirstOrDefault(a => a.BaseStep is not null)?.BaseStep,
			Multipliers = list.FirstOrDefault(a => a.Multipliers?.Length > 0)?.Multipliers,
			VisualStateKey = list.FirstOrDefault(a => !string.IsNullOrEmpty(a.VisualStateKey))?.VisualStateKey,
			Format = list.FirstOrDefault(a => !string.IsNullOrEmpty(a.Format))?.Format,
		};
	}
}
