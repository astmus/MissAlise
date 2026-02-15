using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MissAlise.TelegramBot.Building;
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

	public int GetChoicePage(string paramName)
	{
		var key = $"{KeyChoicePrefix}{paramName}:page";
		var raw = _session.State.TryGetValue(key, out var v) ? v : null;
		return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : 0;
	}

	public void SetChoicePage(string paramName, int page)
	{
		var key = $"{KeyChoicePrefix}{paramName}:page";
		_session.State[key] = page.ToString(CultureInfo.InvariantCulture);
	}

	public static bool TryParseCmdPayload(string payload, out string path)
	{
		path = string.Empty;
		if (!payload.StartsWith("cmd:", StringComparison.OrdinalIgnoreCase))
			return false;
		path = payload.Substring(4);
		path = BotDefinition.NormalizePath(path);
		return path.Length > 0;
	}

	public static bool TryParseSetPayload(string payload, out string paramKey, out string value)
	{
		paramKey = string.Empty;
		value = string.Empty;
		if (!payload.StartsWith("set:", StringComparison.OrdinalIgnoreCase)) return false;

		var rest = payload.Substring(4);
		var i = rest.IndexOf(':');
		if (i <= 0) return false;
		paramKey = rest[..i];
		value = rest[(i + 1)..];
		return paramKey.Length > 0;
	}

	public static bool TryParseAdjustPayload(string payload, out string paramKey, out string delta)
	{
		paramKey = string.Empty;
		delta = string.Empty;
		if (!payload.StartsWith("adj:", StringComparison.OrdinalIgnoreCase)) return false;

		var rest = payload.Substring(4);
		var i = rest.IndexOf(':');
		if (i <= 0) return false;
		paramKey = rest[..i];
		delta = rest[(i + 1)..];
		return paramKey.Length > 0;
	}

	public static bool TryParseChoicePagePayload(string payload, out string paramKey, out int delta)
	{
		paramKey = string.Empty;
		delta = 0;
		if (!payload.StartsWith("choicepage:", StringComparison.OrdinalIgnoreCase)) return false;

		var rest = payload.Substring("choicepage:".Length);
		var i = rest.IndexOf(':');
		if (i <= 0) return false;
		paramKey = rest[..i];
		var deltaStr = rest[(i + 1)..];
		return paramKey.Length > 0 && int.TryParse(deltaStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out delta);
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

	public bool TryApplyValue(
		BotCommandDescription leaf,
		string paramName,
		string raw,
		out string? error)
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

		var optionAlias = option.RawAliases.FirstOrDefault() ?? option.Name;
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
			var raw = GetArg(p.Name!) ?? DefaultValueToRaw(paramDesc?.DefaultValue, p.ParameterType);
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
		var valueMeta = param.DefaultValue; // may contain range/step info
		var targetType = param.ValueType;
		var underlying = Nullable.GetUnderlyingType(targetType);
		if (underlying is not null)
			targetType = underlying;

		var currentRaw = GetArg(param.Name);
		if (targetType == typeof(int))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw)
				? (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<int> nv ? nv.GetDefaultOr(0) : 0)
				: int.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = int.Parse(deltaValue, CultureInfo.InvariantCulture);
			var next = current + delta;
			if (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<int> nv2)
				next = nv2.Clamp(next);
			adjusted = next.ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(long))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw)
				? (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<long> nv ? nv.GetDefaultOr(0L) : 0L)
				: long.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = long.Parse(deltaValue, CultureInfo.InvariantCulture);
			var next = current + delta;
			if (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<long> nv2)
				next = nv2.Clamp(next);
			adjusted = next.ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(double))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw)
				? (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<double> nv ? nv.GetDefaultOr(0.0) : 0.0)
				: double.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = double.Parse(deltaValue, CultureInfo.InvariantCulture);
			var next = current + delta;
			if (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<double> nv2)
				next = nv2.Clamp(next);
			adjusted = next.ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(decimal))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw)
				? (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<decimal> nv ? nv.GetDefaultOr(0m) : 0m)
				: decimal.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = decimal.Parse(deltaValue, CultureInfo.InvariantCulture);
			var next = current + delta;
			if (valueMeta is MissAlise.TelegramBot.Building.Values.NumericValue<decimal> nv2)
				next = nv2.Clamp(next);
			adjusted = next.ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(TimeSpan))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw) ? TimeSpan.Zero : TimeSpan.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = TimeSpan.Parse(deltaValue, CultureInfo.InvariantCulture);
			adjusted = (current + delta).ToString("c", CultureInfo.InvariantCulture);
			return true;
		}

		return false;
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
