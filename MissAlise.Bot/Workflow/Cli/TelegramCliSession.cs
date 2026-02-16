using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using MissAlise.TelegramBot.Building;
using MissAlise.TelegramBot.Building.Values;
using MissAlise.Workflow;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Обёртка над <see cref="WorkflowSession"/> для Telegram CLI workflow (menu + wizard + command review).
/// Движок workflow не знает режимы. Всё хранится в <see cref="WorkflowSession.State"/>.
/// </summary>
internal sealed class TelegramCliSession
{
	private const string KeyPath = "cli:path";
	private const string KeyParamIndex = "cli:paramIndex";
	private const string KeyError = "cli:error";
	private const string KeyChoicePrefix = "cli:choice:";
	private const string ArgPrefix = "arg:";

	private readonly WorkflowSession _session;

	public TelegramCliSession(WorkflowSession session)
	{
		_session = session ?? throw new ArgumentNullException(nameof(session));
	}

	/// <summary>Базовая сессия workflow.</summary>
	public WorkflowSession Session => _session;

	/// <summary>Путь команды (cli:path).</summary>
	public string? Path
	{
		get => _session.Get(KeyPath);
		set => _session.Set(KeyPath, value ?? string.Empty);
	}

	/// <summary>Индекс текущего параметра (cli:paramIndex).</summary>
	public int ParamIndex
	{
		get => _session.GetInt(KeyParamIndex, 0);
		set => _session.Set(KeyParamIndex, value.ToString(CultureInfo.InvariantCulture));
	}

	/// <summary>Сообщение об ошибке (cli:error).</summary>
	public string? Error
	{
		get => _session.Get(KeyError);
		set => _session.Set(KeyError, value ?? string.Empty);
	}

	/// <summary>Получить значение аргумента по имени параметра.</summary>
	public string? GetArg(string paramName) => _session.Get(ArgPrefix + paramName);

	/// <summary>Установить значение аргумента по имени параметра.</summary>
	public void SetArg(string paramName, string value) => _session.Set(ArgPrefix + paramName, value);

	/// <summary>Удалить сообщение об ошибке из состояния.</summary>
	public void RemoveError() => _session.State.Remove(KeyError);

	/// <summary>Удалить индекс параметра из состояния.</summary>
	public void RemoveParamIndex() => _session.State.Remove(KeyParamIndex);

	/// <summary>Создать словарь обновлений состояния для передачи в WorkflowStepResult.</summary>
	public Dictionary<string, string> BuildStateUpdate(string? error = null, string? path = null, int? paramIndex = null)
	{
		var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (error != null) d[KeyError] = error;
		if (path != null) d[KeyPath] = path;
		if (paramIndex.HasValue) d[KeyParamIndex] = paramIndex.Value.ToString(CultureInfo.InvariantCulture);
		return d;
	}

	public void ClearArgs()
	{
		var keys = _session.State.Keys
			.Where(k => k.StartsWith(ArgPrefix, StringComparison.OrdinalIgnoreCase))
			.ToArray();

		foreach (var k in keys)
			_session.State.Remove(k);
	}

	public void ResetAll()
	{
		ClearArgs();
		_session.State.Remove(KeyPath);
		_session.State.Remove(KeyParamIndex);
		_session.State.Remove(KeyError);
		// remove choice paging keys
		var choiceKeys = _session.State.Keys.Where(k => k.StartsWith(KeyChoicePrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (var k in choiceKeys)
			_session.State.Remove(k);
	}

	public int GetChoice(string paramName)
	{
		var key = $"{KeyChoicePrefix}{paramName}:page";
		var raw = _session.State.TryGetValue(key, out var v) ? v : null;
		return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : 0;
	}

	public void SetChoice(string paramName, int page)
	{
		var key = $"{KeyChoicePrefix}{paramName}:page";
		_session.State[key] = page.ToString(CultureInfo.InvariantCulture);
	}

	public bool AllRequiredCollected(BotCommandDescription leaf)
		=> leaf.Parameters
			.Where(p => p.IsRequired)
			.All(p => !string.IsNullOrWhiteSpace(GetArg(p.Name)));

	public int NextMissingRequiredIndex(BotCommandDescription leaf)
	{
		for (var i = 0; i < leaf.Parameters.Count; i++)
		{
			var p = leaf.Parameters[i];
			if (p.IsRequired && string.IsNullOrWhiteSpace(GetArg(p.Name)))
				return i;
		}
		return -1;
	}

	public bool TryApplyValue(BotCommandDescription leaf, string paramName, string raw, out string? error)
	{
		error = null;
		var param = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));
		if (param is null)
		{
			error = $"Неизвестный параметр '{paramName}'.";
			return false;
		}

		if (leaf.OptionsByParamKey is null || !leaf.OptionsByParamKey.TryGetValue(param.Name, out var option))
		{
			error = $"Для параметра '{paramName}' не найден CLI option.";
			return false;
		}

		if (!TryValidateValue(leaf, option, raw, out error))
			return false;

		SetArg(param.Name, raw);
		return true;
	}

	public static bool TryValidateValue(BotCommandDescription leaf, Option option, string raw, out string? error)
	{
		error = null;

		if (leaf.Command is null)
		{
			error = "Не найдена команда для валидации параметра.";
			return false;
		}

		var optionAlias = option.Aliases.FirstOrDefault() ?? option.Name;
		var escapedValue = EscapeCliValue(raw);
		var parseResult = leaf.Command.Parse($"{optionAlias} {escapedValue}");
		if (parseResult.Errors.Count == 0)
		{
			return true;
		}

		error = string.Join(" ", parseResult.Errors.Select(e => e.Message));
		return false;
	}

	private static string EscapeCliValue(string value)
		=> '"' + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + '"';

	private static readonly ConcurrentDictionary<Type, ParameterInfo[]> _ctorParamsCache = new();

	public object CreateCommandFromSession(BotCommandDescription leaf)
	{
		var type = leaf.CommandType;
		var ctorParams = _ctorParamsCache.GetOrAdd(type, t => t.GetConstructors().Single().GetParameters());
		var args = new object?[ctorParams.Length];

		for (int i = 0; i < ctorParams.Length; i++)
		{
			var p = ctorParams[i];
			var paramDesc = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
			var raw = GetArg(p.Name!) ?? DefaultValueToRaw(paramDesc?.Value, p.ParameterType);
			if (raw is null)
				throw new InvalidOperationException($"Missing session value for '{p.Name}'");

			args[i] = ConvertFromString(raw, p.ParameterType);
		}

		return Activator.CreateInstance(type, args)
			?? throw new InvalidOperationException($"Failed to create instance of '{type.FullName}'");
	}

	public bool TryAdjustNumeric(BotParameterDescription param, string deltaValue, out string adjusted)
	{
		adjusted = string.Empty;
		if (param.Value is not null && TryAdjustByValueMeta(param, param.Value, deltaValue, out adjusted))
			return true;

		return TryAdjustByTargetTypeFallback(param, deltaValue, out adjusted);
	}

	private bool TryAdjustByValueMeta(BotParameterDescription param, ValueBase valueMeta, string deltaRaw, out string adjusted)
	{
		switch (valueMeta)
		{
			case Value<int> vInt:
				return TryAdjustTypedValue(param, vInt, deltaRaw, 0, out adjusted);
			case Value<long> vLong:
				return TryAdjustTypedValue(param, vLong, deltaRaw, 0L, out adjusted);
			case Value<double> vDouble:
				return TryAdjustTypedValue(param, vDouble, deltaRaw, 0.0, out adjusted);
			case Value<decimal> vDecimal:
				return TryAdjustTypedValue(param, vDecimal, deltaRaw, 0m, out adjusted);
			case Value<TimeSpan> vTimeSpan:
				return TryAdjustTypedValue(param, vTimeSpan, deltaRaw, TimeSpan.Zero, out adjusted);
			default:
				adjusted = string.Empty;
				return false;
		}
	}

	private bool TryAdjustByTargetTypeFallback(BotParameterDescription param, string deltaRaw, out string adjusted)
	{
		adjusted = string.Empty;
		var targetType = Nullable.GetUnderlyingType(param.ValueType) ?? param.ValueType;
		if (targetType == typeof(int))
			return TryAdjustPlainNumeric<int>(param, deltaRaw, out adjusted);
		if (targetType == typeof(long))
			return TryAdjustPlainNumeric<long>(param, deltaRaw, out adjusted);
		if (targetType == typeof(double))
			return TryAdjustPlainNumeric<double>(param, deltaRaw, out adjusted);
		if (targetType == typeof(decimal))
			return TryAdjustPlainNumeric<decimal>(param, deltaRaw, out adjusted);
		if (targetType == typeof(TimeSpan))
			return TryAdjustPlainTimeSpan(param, deltaRaw, out adjusted);
		return false;
	}

	private bool TryAdjustTypedValue<T>(BotParameterDescription param, Value<T> valueMeta, string deltaRaw, T fallbackDefault, out string adjusted)
		where T : IParsable<T>
	{
		adjusted = string.Empty;
		var currentRaw = GetArg(param.Name);

		if (!TryParseOrDefault(valueMeta, currentRaw, fallbackDefault, out var current))
			return false;
		if (!valueMeta.TryParseRaw(deltaRaw, out var delta))
			return false;

		T next;
		try
		{
			next = valueMeta.Next(current, delta);
		}
		catch (NotSupportedException)
		{
			return false;
		}

		adjusted = valueMeta.ToRaw(next);
		return true;
	}

	private bool TryAdjustPlainNumeric<T>(BotParameterDescription param, string deltaRaw, out string adjusted)
		where T : struct, INumber<T>, IParsable<T>
	{
		adjusted = string.Empty;
		var parser = new Value<T>();
		var currentRaw = GetArg(param.Name);
		if (!TryParseOrDefault(parser, currentRaw, T.AdditiveIdentity, out var current))
			return false;
		if (!parser.TryParseRaw(deltaRaw, out var delta))
			return false;

		var next = current + delta;
		adjusted = parser.ToRaw(next);
		return true;
	}

	private bool TryAdjustPlainTimeSpan(BotParameterDescription param, string deltaRaw, out string adjusted)
	{
		adjusted = string.Empty;
		var parser = new Value<TimeSpan>();
		var currentRaw = GetArg(param.Name);
		if (!TryParseOrDefault(parser, currentRaw, TimeSpan.Zero, out var current))
			return false;
		if (!parser.TryParseRaw(deltaRaw, out var delta))
			return false;

		adjusted = parser.ToRaw(current + delta);
		return true;
	}

	private static bool TryParseOrDefault<T>(Value<T> valueMeta, string? raw, T fallbackDefault, out T value)
		where T : IParsable<T>
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			value = valueMeta.GetDefaultOr(fallbackDefault);
			return true;
		}

		return valueMeta.TryParseRaw(raw, out value);
	}

	private static object? ConvertFromString(string raw, Type targetType)
	{
		var underlying = Nullable.GetUnderlyingType(targetType);
		if (underlying is not null)
		{
			if (string.IsNullOrWhiteSpace(raw) || raw == "-")
				return null;
			targetType = underlying;
		}

		if (targetType == typeof(string))
			return raw;

		if (targetType == typeof(Guid))
		{
			if (Guid.TryParse(raw, out var g)) return g;
			throw new FormatException($"Value '{raw}' is not a valid GUID.");
		}

		if (targetType == typeof(bool))
		{
			if (bool.TryParse(raw, out var b)) return b;
			if (raw == "1" || raw.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
			if (raw == "0" || raw.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
			throw new FormatException($"Value '{raw}' is not a valid boolean.");
		}

		if (targetType.IsEnum)
		{
			if (Enum.TryParse(targetType, raw, ignoreCase: true, out var e))
				return e;
			throw new FormatException($"Value '{raw}' is not valid for enum '{targetType.Name}'.");
		}

		if (targetType == typeof(int))
			return int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (targetType == typeof(long))
			return long.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);
		if (targetType == typeof(double))
			return double.Parse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
		if (targetType == typeof(decimal))
			return decimal.Parse(raw, NumberStyles.Number, CultureInfo.InvariantCulture);
		if (targetType == typeof(DateTime))
			return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		if (targetType == typeof(TimeSpan))
			return TimeSpan.Parse(raw, CultureInfo.InvariantCulture);

		var conv = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
		if (conv.CanConvertFrom(typeof(string)))
			return conv.ConvertFromInvariantString(raw);

		throw new NotSupportedException($"Cannot convert string to '{targetType.FullName}'.");
	}

	private static string? DefaultValueToRaw(object? defaultValue, Type targetType)
	{
		if (defaultValue is MissAlise.TelegramBot.Building.Values.ValueBase vb)
			defaultValue = vb.Default;
		if (defaultValue is null) return null;
		if (defaultValue is string s) return s;
		if (defaultValue is int or long or double or decimal)
			return Convert.ToString(defaultValue, CultureInfo.InvariantCulture);
		if (defaultValue is DateTime dt)
			return dt.ToString("o", CultureInfo.InvariantCulture);
		if (defaultValue is TimeSpan ts)
			return ts.ToString("c", CultureInfo.InvariantCulture);
		if (defaultValue is bool b) return b ? "true" : "false";
		if (defaultValue.GetType().IsEnum)
			return defaultValue.ToString();
		return defaultValue.ToString();
	}
}
