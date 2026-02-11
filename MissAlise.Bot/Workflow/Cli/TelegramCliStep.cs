using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Steps;
using static System.Net.Mime.MediaTypeNames;
using BotCommandDescription = MissAlise.TelegramBot.Building.BotCommandDescription;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Единый step для Telegram:
/// - парсинг /команд через System.CommandLine (BotDefinition)
/// - wizard-сессия для добора параметров (text/callback)
/// </summary>
internal sealed class TelegramCliStep : IWorkflowStep
{
	private readonly BotDefinition _botDescription;

	public TelegramCliStep(BotDefinition bot)
	{
		_botDescription = bot;
	}

	private const string S_Mode = "cli:mode"; // menu|wizard
	private const string S_Path = "cli:path"; // command path
	private const string S_ParamIndex = "cli:paramIndex";
	private const string S_Error = "cli:error";

	private static string ArgKey(string paramName) => "arg:" + paramName;

	public Task<WorkflowStepResult> ExecuteAsync(
		WorkflowContext context,
		WorkflowSession session,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		session.State.Remove(S_Error);

		var mode = session.Get(S_Mode) ?? "menu";

		// Global cancel
		if (IsCancel(input))
			return Task.FromResult(ResetWizard(session));

		// Wizard has priority
		if (mode == "wizard")
			return Task.FromResult(HandleWizard(session, input));

		// Menu/command mode
		return Task.FromResult(HandleMenuOrCommand(session, input));
	}

	private static bool IsCancel(WorkflowInput input)
	{
		if (input.Kind == WorkflowInputKind.Callback && (input.Payload?.Equals("nav:cancel", StringComparison.OrdinalIgnoreCase) == true))
			return true;
		if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
			return string.Equals(input.Text?.Trim(), "/cancel", StringComparison.OrdinalIgnoreCase);
		return false;
	}

	private WorkflowStepResult HandleMenuOrCommand(WorkflowSession session, WorkflowInput input)
	{
		// Callback navigation to a command path
		if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
		{
			if (TryParseCmdPayload(input.Payload, out var path))
				return StartCommandOrWizard(session, path);
		}

		// Command line text
		if (input.Kind is WorkflowInputKind.Command or WorkflowInputKind.Text)
		{
			var text = input.Text?.Trim();
			if (string.IsNullOrWhiteSpace(text))
				return DisplayMenu(session);

			// allow pasted callback payload
			if (TryParseCmdPayload(text, out var cbPath))
				return StartCommandOrWizard(session, cbPath);

			// not a command -> fallback menu
			if (!text.StartsWith('/'))
				return DisplayMenu(session, hint: "Я работаю командами. Выбери команду ниже или введи /start");

			// Parse via System.CommandLine			
			var parse = _botDescription.RootCommand.Parse(text);
			if (!_botDescription.TryResolveLeaf(parse, out var leaf))
			{
				// Not a leaf: show nearest menu based on tokens (e.g. /onedrive)
				var path = string.Join(' ', parse.CommandResult.Command.Options.Where(a => !a.Name.StartsWith('-')).Select(a => a.Name.TrimStart('/')));
				return DisplayMenu(session, path);
			}

			var missing = _botDescription.GetMissing(parse, leaf);
			if (missing.Count > 0)
			{
				// start wizard: store path + prefilled values
				session.Set(S_Mode, "wizard");
				session.Set(S_Path, leaf.Path);
				session.Set(S_ParamIndex, "0");

				// save provided options into session state (as strings)
				foreach (var p in leaf.Parameters)
				{
					var opt = leaf.OptionsByParamKey[p.Name];
					var optRes = parse.CommandResult.FindResultFor(opt);
					if (optRes is null) continue;
					if (optRes.Tokens.Count == 0) continue;

					session.Set(ArgKey(p.Name), string.Join(' ', optRes.Tokens.Select(t => t.Value)));
				}

				// jump to first missing required param
				var firstMissing = missing[0].Param;
				var idx = leaf.Parameters.FindIndex(x => x.Name.Equals(firstMissing.Name, StringComparison.OrdinalIgnoreCase));
				session.Set(S_ParamIndex, Math.Max(idx, 0).ToString());

				return WorkflowStepResult.Stay(new Dictionary<string, string>
				{
					[S_Mode] = "wizard",
					[S_Path] = leaf.Path,
					[S_ParamIndex] = session.Get(S_ParamIndex)!
				});
			}

			var model = _botDescription.BindModel(parse, leaf);
			return WorkflowStepResult.Produce(model);
		}

		return DisplayMenu(session);
	}

	private WorkflowStepResult StartCommandOrWizard(WorkflowSession session, string path)
	{
		// If it's a leaf path -> start wizard immediately (no args) or execute if no params
		if (_botDescription.TryGetLeafByPath(path, out var leafDesc) && leafDesc.SubCommands.Count == 0)
		{
			if (leafDesc.Parameters.Count == 0)
			{
				var cmd = Activator.CreateInstance(leafDesc.CommandType!)!;
				return WorkflowStepResult.Complete(cmd);
			}

			return WorkflowStepResult.Stay(new Dictionary<string, string>
			{
				[S_Mode] = "wizard",
				[S_Path] = BotDefinition.NormalizePath(path),
				[S_ParamIndex] = "0"
			});
		}

		return DisplayMenu(session, path);
	}

	private WorkflowStepResult HandleWizard(WorkflowSession session, WorkflowInput input)
	{
		var path = session.Get(S_Path);
		if (!_botDescription.TryGetLeafByPath(path, out var leaf))
			return DisplayMenu(session);

		var idx = session.GetInt(S_ParamIndex, 0);
		if (idx < 0) idx = 0;
		if (idx >= leaf.Parameters.Count)
			idx = leaf.Parameters.Count - 1;

		var inputData = input.Payload;

		if (input.Kind is WorkflowInputKind.Text)
		{
			inputData = $"set:{leaf.Parameters[idx].Name}:{input.Text?.Trim()}";
		}

		if (GetCallbackType(inputData) is not string cbType)
			return WorkflowStepResult.Stay();

		if (TryParseSetPayload(inputData, out var pKey, out var pValue))
		{
			if (!TryApplyValue(leaf, session, pKey, pValue, out var error))
			{
				return WorkflowStepResult.Stay(new Dictionary<string, string>
				{
					[S_Mode] = "wizard",
					[S_Path] = path,
					[S_ParamIndex] = idx.ToString(),
					[S_Error] = error
				});
			}

			if (AllRequiredCollected(leaf, session))
			{
				var cmd = CreateCommandFromSession(leaf, session);
				ResetWizard(session);
				return WorkflowStepResult.Complete(cmd);
			}

			idx = NextIndex(leaf, session);
		}

		if (TryParseAdjustPayload(inputData, out var adjKey, out var deltaValue))
		{
			var param = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(adjKey, StringComparison.OrdinalIgnoreCase));
			param ??= leaf.Parameters[idx];
			if (TryAdjustNumeric(session, param, deltaValue, out var adjusted))
			{
				session.Set(ArgKey(param.Name), adjusted);
			}			
		}

		if (inputData.Equals("nav:accept", StringComparison.OrdinalIgnoreCase))
		{
			var curParam = leaf.Parameters[idx];
			var currentValue = session.Get(ArgKey(curParam.Name));
			if (!string.IsNullOrWhiteSpace(currentValue))
			{
				idx = NextIndex(leaf, session);
				session.Set(S_ParamIndex, idx.ToString());

				if (AllRequiredCollected(leaf, session))
				{
					var cmd = CreateCommandFromSession(leaf, session);
					var reset = ResetWizard(session);
					return WorkflowStepResult.Produce(cmd, updates: reset.StateUpdates);
				}
			}

		}

		return WorkflowStepResult.Stay(new Dictionary<string, string>
		{
			[S_Mode] = "wizard",
			[S_Path] = path,
			[S_ParamIndex] = idx.ToString()//,
			//[S_CTX_MESSAGE] = context,
		});
	}

	private static string GetCallbackType(string text) => text switch
	{
		var t when t.StartsWith("cmd:", StringComparison.OrdinalIgnoreCase) => "cmd",
		var t when t.StartsWith("set:", StringComparison.OrdinalIgnoreCase) => "set",
		var t when t.StartsWith("adj:", StringComparison.OrdinalIgnoreCase) => "adj",
		var t when t.StartsWith("nav:", StringComparison.OrdinalIgnoreCase) => "nav",

		_ => null
	};

	private static bool TryApplyValue(BotCommandDescription leaf, WorkflowSession session, string paramName, string raw, out string? error)
	{
		error = null;
		var param = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));
		if (param is null)
		{
			error = $"Неизвестный параметр '{paramName}'.";
			session.Set(S_Error, error);
			return false;
		}

		if (!TryValidateValue(param, raw, out error))
		{
			session.Set(S_Error, error ?? "Некорректное значение.");
			return false;
		}

		session.Set(ArgKey(param.Name), raw);
		return true;
	}

	private static bool TryValidateValue(BotParameterDescription param, string raw, out string? error)
	{
		error = null;
		try
		{
			var value = ConvertFromString(raw, param.ValueType);
			//if (value is null && param.RangeMin is null && param.RangeMax is null)
			//	return true;
			//if (value is not null && !IsInRange(value, param.RangeMin, param.RangeMax, out error))
			//	return false;
			return true;
		}
		catch (Exception ex) when (ex is FormatException or NotSupportedException or ArgumentException)
		{
			error = ex.Message;
			return false;
		}
	}

	private static bool AllRequiredCollected(BotCommandDescription leaf, WorkflowSession session)
		=> leaf.Parameters.Where(p => p.IsRequired).All(p => !string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))));

	private static int NextIndex(BotCommandDescription leaf, WorkflowSession session)
	{
		for (var i = 0; i < leaf.Parameters.Count; i++)
		{
			var p = leaf.Parameters[i];
			if (p.IsRequired && string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))))
				return i;
		}

		return -1;
	}

	private static readonly ConcurrentDictionary<Type, ParameterInfo[]> _ctorParamsCache = new();

	private static object CreateCommandFromSession(BotCommandDescription leaf, WorkflowSession session)
	{
		var type = leaf.CommandType;

		// контракт: record с одним primary ctor (public)
		var ctorParams = _ctorParamsCache.GetOrAdd(
			type,
			t => t.GetConstructors().Single().GetParameters());

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

	private static object? ConvertFromString(string raw, Type targetType)
	{
		// nullable
		var underlying = Nullable.GetUnderlyingType(targetType);
		if (underlying is not null)
		{
			// "" или "-" можно трактовать как null (если хочешь)
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
			// поддержим "1/0", "yes/no", "true/false"
			if (bool.TryParse(raw, out var b)) return b;
			if (raw == "1" || raw.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
			if (raw == "0" || raw.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
			throw new FormatException($"Value '{raw}' is not a valid boolean.");
		}

		if (targetType.IsEnum)
		{
			// и по имени, и по числу
			if (Enum.TryParse(targetType, raw, ignoreCase: true, out var e))
				return e;

			throw new FormatException($"Value '{raw}' is not valid for enum '{targetType.Name}'.");
		}

		// числа/даты — инвариантная культура
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

		// fallback через TypeConverter (например, Uri и т.п.)
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

	private WorkflowStepResult DisplayMenu(WorkflowSession session, string path = null, string hint = null)
	{
		session.Set(S_Path, path);
		session.Set(S_Error, hint);
		session.State.Remove(S_ParamIndex);

		return WorkflowStepResult.Stay(new Dictionary<string, string>
		{
			[S_Mode] = "menu",
		});
	}

	private static WorkflowStepResult ResetWizard(WorkflowSession session)
	{
		// Remove all arg:* keys
		var keys = session.State.Keys.Where(k => k.StartsWith("arg:", StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (var k in keys) session.State.Remove(k);

		session.State.Remove(S_Mode);
		session.State.Remove(S_Path);
		session.State.Remove(S_ParamIndex);
		session.State.Remove(S_Error);

		return WorkflowStepResult.Stay();
	}

	private static bool TryAdjustNumeric(WorkflowSession session, BotParameterDescription param, string deltaValue, out string adjusted)
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

		if (targetType == typeof(DateTime))
		{
			var current = string.IsNullOrWhiteSpace(currentRaw) ? DateTime.MinValue : DateTime.Parse(currentRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
			var deltaDt = DateTime.Parse(deltaValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
			var delta = deltaDt - DateTime.MinValue;
			adjusted = (current + delta).ToString("o", CultureInfo.InvariantCulture);
			return true;
		}

		return false;
	}

	private static bool TryParseCmdPayload(string payload, out string path)
	{
		path = string.Empty;
		if (!payload.StartsWith("cmd:", StringComparison.OrdinalIgnoreCase)) return false;
		path = payload.Substring(4);
		path = BotDefinition.NormalizePath(path);
		return path.Length > 0;
	}

	private static bool TryParseSetPayload(string payload, out string paramKey, out string value)
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

	private static bool TryParseAdjustPayload(string payload, out string paramKey, out string delta)
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
}
