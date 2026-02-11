using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Общие ключи/утилиты для Telegram CLI workflow (menu + wizard + command review).
/// Движок workflow не знает режимы. Всё хранится в <see cref="WorkflowSession.State"/>.
/// </summary>
internal static class TelegramCliSession
{
	public const string S_Path = "cli:path"; // command path
	public const string S_ParamIndex = "cli:paramIndex";
	public const string S_Error = "cli:error";

	public static string ArgKey(string paramName) => "arg:" + paramName;

	public static void ClearArgs(WorkflowSession session)
	{
		var keys = session.State.Keys
			.Where(k => k.StartsWith("arg:", StringComparison.OrdinalIgnoreCase))
			.ToArray();
		foreach (var k in keys)
			session.State.Remove(k);
	}

	public static void ResetAll(WorkflowSession session)
	{
		ClearArgs(session);
		session.State.Remove(S_Path);
		session.State.Remove(S_ParamIndex);
		session.State.Remove(S_Error);
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

	public static bool AllRequiredCollected(BotCommandDescription leaf, WorkflowSession session)
		=> leaf.Parameters
			.Where(p => p.IsRequired)
			.All(p => !string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))));

	public static int NextMissingRequiredIndex(BotCommandDescription leaf, WorkflowSession session)
	{
		for (var i = 0; i < leaf.Parameters.Count; i++)
		{
			var p = leaf.Parameters[i];
			if (p.IsRequired && string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))))
				return i;
		}
		return -1;
	}

	public static bool TryApplyValue(
		BotCommandDescription leaf,
		WorkflowSession session,
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

		if (!TryValidateValue(param, raw, out error))
			return false;

		session.Set(ArgKey(param.Name), raw);
		return true;
	}

	public static bool TryValidateValue(BotParameterDescription param, string raw, out string? error)
	{
		error = null;
		try
		{
			_ = ConvertFromString(raw, param.ValueType);
			return true;
		}
		catch (Exception ex) when (ex is FormatException or NotSupportedException or ArgumentException)
		{
			error = ex.Message;
			return false;
		}
	}

	private static readonly ConcurrentDictionary<Type, ParameterInfo[]> _ctorParamsCache = new();

	public static object CreateCommandFromSession(BotCommandDescription leaf, WorkflowSession session)
	{
		var type = leaf.CommandType;
		var ctorParams = _ctorParamsCache.GetOrAdd(type, t => t.GetConstructors().Single().GetParameters());
		var args = new object?[ctorParams.Length];

		for (int i = 0; i < ctorParams.Length; i++)
		{
			var p = ctorParams[i];
			var paramDesc = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
			var raw = session.Get(ArgKey(p.Name!)) ?? DefaultValueToRaw(paramDesc?.DefaultValue, p.ParameterType);
			if (raw is null)
				throw new InvalidOperationException($"Missing session value for '{p.Name}'");

			args[i] = ConvertFromString(raw, p.ParameterType);
		}

		return Activator.CreateInstance(type, args)
			?? throw new InvalidOperationException($"Failed to create instance of '{type.FullName}'");
	}

	public static bool TryAdjustNumeric(WorkflowSession session, BotParameterDescription param, string deltaValue, out string adjusted)
	{
		adjusted = string.Empty;
		var targetType = param.ValueType;
		var underlying = Nullable.GetUnderlyingType(targetType);
		if (underlying is not null)
			targetType = underlying;

		var currentRaw = session.Get(ArgKey(param.Name));
		if (targetType == typeof(int))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw) ? 0 : int.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = int.Parse(deltaValue, CultureInfo.InvariantCulture);
			adjusted = (current + delta).ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(long))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw) ? 0L : long.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = long.Parse(deltaValue, CultureInfo.InvariantCulture);
			adjusted = (current + delta).ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(double))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw) ? 0.0 : double.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = double.Parse(deltaValue, CultureInfo.InvariantCulture);
			adjusted = (current + delta).ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (targetType == typeof(decimal))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw) ? 0m : decimal.Parse(currentRaw, CultureInfo.InvariantCulture);
			var delta = decimal.Parse(deltaValue, CultureInfo.InvariantCulture);
			adjusted = (current + delta).ToString(CultureInfo.InvariantCulture);
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
