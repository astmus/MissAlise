using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MissAlise.TelegramBot.Building;
using MissAlise.TelegramBot.CommandLine;
using MissAlise.Workflow;
using MissAlise.Workflow.Steps;
using BotCommandDescription = MissAlise.TelegramBot.Building.BotCommandDescription;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Единый step для Telegram:
/// - парсинг /команд через System.CommandLine (BotDefinition)
/// - wizard-сессия для добора параметров (text/callback)
/// </summary>
internal sealed class TelegramCliStep : IWorkflowStep
{
	private readonly BotDefinition _bot;

	public TelegramCliStep(BotDefinition bot)
	{
		_bot = bot;
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
			return Task.FromResult(ResetToMenu(session));

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
				return StartCommandOrMenu(session, path);
		}

		// Command line text
		if (input.Kind is WorkflowInputKind.Command or WorkflowInputKind.Text)
		{
			var text = input.Text?.Trim();
			if (string.IsNullOrWhiteSpace(text))
				return ShowRootMenu(session);

			// allow pasted callback payload
			if (LooksLikeCallbackText(text) && TryParseCmdPayload(text, out var cbPath))
				return StartCommandOrMenu(session, cbPath);

			// not a command -> fallback menu
			if (!text.StartsWith('/'))
				return ShowRootMenu(session, hint: "Я работаю командами. Выбери команду ниже или введи /start");

			// Parse via System.CommandLine			
			var parse = _bot.RootCommand.Parse(text);
			if (!_bot.TryResolveLeaf(parse, out var leaf))
			{
				// Not a leaf: show nearest menu based on tokens (e.g. /onedrive)
				var path = string.Join(' ', parse.CommandResult.Command.Options.Where(a => !a.Name.StartsWith('-')).Select(a => a.Name.TrimStart('/')));
				return ShowMenuForPath(session, path);
			}

			var missing = _bot.GetMissing(parse, leaf);
			if (missing.Count > 0)
			{
				// start wizard: store path + prefilled values
				session.Set(S_Mode, "wizard");
				session.Set(S_Path, leaf.Path);
				session.Set(S_ParamIndex, "0");

				// save provided options into session state (as strings)
				foreach (var p in leaf.Description.Parameters)
				{
					var opt = leaf.OptionsByParamKey[p.Name];
					var optRes = parse.CommandResult.FindResultFor(opt);
					if (optRes is null) continue;
					if (optRes.Tokens.Count == 0) continue;

					session.Set(ArgKey(p.Name), string.Join(' ', optRes.Tokens.Select(t => t.Value)));
				}

				// jump to first missing required param
				var firstMissing = missing[0].Param;
				var idx = leaf.Description.Parameters.FindIndex(x => x.Name.Equals(firstMissing.Name, StringComparison.OrdinalIgnoreCase));
				session.Set(S_ParamIndex, Math.Max(idx, 0).ToString());

				return WorkflowStepResult.Stay(new Dictionary<string, string>
				{
					[S_Mode] = "wizard",
					[S_Path] = leaf.Path,
					[S_ParamIndex] = session.Get(S_ParamIndex)!
				});
			}

			var model = _bot.BindModel(parse, leaf);
			return WorkflowStepResult.Produce(model);
		}

		return ShowRootMenu(session);
	}

	private WorkflowStepResult StartCommandOrMenu(WorkflowSession session, string path)
	{
		// If it's a leaf path -> start wizard immediately (no args) or execute if no params
		if (_bot.TryGetLeafByPath(path, out var leafDesc))
		{
			if (leafDesc.Parameters.Count == 0)
			{
				var cmd = Activator.CreateInstance(leafDesc.CommandType!)!;
				return WorkflowStepResult.Produce(cmd);
			}

			session.Set(S_Mode, "wizard");
			session.Set(S_Path, BotDefinition.NormalizePath(path));
			session.Set(S_ParamIndex, "0");
			return WorkflowStepResult.Stay(new Dictionary<string, string>
			{
				[S_Mode] = "wizard",
				[S_Path] = BotDefinition.NormalizePath(path),
				[S_ParamIndex] = "0"
			});
		}

		// otherwise show menu at this path
		return ShowMenuForPath(session, path);
	}

	private WorkflowStepResult HandleWizard(WorkflowSession session, WorkflowInput input)
	{
		var path = session.Get(S_Path);
		if (string.IsNullOrWhiteSpace(path) || !_bot.TryGetLeafByPath(path, out var leaf))
			return ResetToMenu(session);

		var idx = session.GetInt(S_ParamIndex, 0);
		if (idx < 0) idx = 0;
		if (idx >= leaf.Parameters.Count)
			idx = leaf.Parameters.Count - 1;

		// Apply callback set:param:value
		if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
		{
			if (TryParseSetPayload(input.Payload, out var pKey, out var pValue))
			{
				session.Set(ArgKey(pKey), pValue);
				idx = NextIndex(leaf, session, idx);
				session.Set(S_ParamIndex, idx.ToString());
				return WorkflowStepResult.Stay(new Dictionary<string, string>
				{
					[S_Mode] = "wizard",
					[S_Path] = path,
					[S_ParamIndex] = idx.ToString()
				});
			}
		}

		// Apply plain text as current param value
		if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
		{
			var text = input.Text?.Trim();
			if (!string.IsNullOrWhiteSpace(text))
			{
				// allow pasted callback set:
				if (LooksLikeCallbackText(text) && TryParseSetPayload(text, out var pk2, out var pv2))
				{
					session.Set(ArgKey(pk2), pv2);
				}
				else
				{
					var curParam = leaf.Parameters[idx];
					session.Set(ArgKey(curParam.Name), text);
				}

				idx = NextIndex(leaf, session, idx);
				session.Set(S_ParamIndex, idx.ToString());

				// All required collected?
				if (AllRequiredCollected(leaf, session))
				{
					var cmd = CreateCommandFromSession(leaf, session);
					// reset wizard state
					var reset = ResetToMenu(session);
					return WorkflowStepResult.Produce(cmd, next: null, updates: reset.StateUpdates);
				}

				return WorkflowStepResult.Stay(new Dictionary<string, string>
				{
					[S_Mode] = "wizard",
					[S_Path] = path,
					[S_ParamIndex] = idx.ToString()
				});
			}
		}

		return WorkflowStepResult.Stay(null);
	}

	private static bool AllRequiredCollected(BotCommandDescription leaf, WorkflowSession session)
		=> leaf.Parameters.Where(p => p.IsRequired).All(p => !string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))));

	private static int NextIndex(BotCommandDescription leaf, WorkflowSession session, int currentIdx)
	{
		// move to next missing required
		for (var i = currentIdx + 1; i < leaf.Parameters.Count; i++)
		{
			var p = leaf.Parameters[i];
			if (p.IsRequired && string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))))
				return i;
		}
		// or first missing required
		for (var i = 0; i < leaf.Parameters.Count; i++)
		{
			var p = leaf.Parameters[i];
			if (p.IsRequired && string.IsNullOrWhiteSpace(session.Get(ArgKey(p.Name))))
				return i;
		}
		return Math.Clamp(currentIdx, 0, Math.Max(0, leaf.Parameters.Count - 1));
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

			var raw = session.Get(ArgKey(p.Name!)); // string?
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

	private WorkflowStepResult ShowRootMenu(WorkflowSession session, string? hint = null)
	{
		session.Set(S_Mode, "menu");
		session.State.Remove(S_Path);
		session.State.Remove(S_ParamIndex);
		if (hint is not null)
			session.Set(S_Error, hint);

		return WorkflowStepResult.Stay(new Dictionary<string, string>
		{
			[S_Mode] = "menu",
			[S_Error] = hint ?? string.Empty
		});
	}

	private WorkflowStepResult ShowMenuForPath(WorkflowSession session, string path)
	{
		path = BotDefinition.NormalizePath(path);
		session.Set(S_Mode, "menu");
		session.Set(S_Path, path);

		return WorkflowStepResult.Stay(new Dictionary<string, string>
		{
			[S_Mode] = "menu",
			[S_Path] = path
		});
	}

	private static WorkflowStepResult ResetToMenu(WorkflowSession session)
	{
		// Remove all arg:* keys
		var keys = session.State.Keys.Where(k => k.StartsWith("arg:", StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (var k in keys) session.State.Remove(k);

		session.State.Remove(S_Path);
		session.State.Remove(S_ParamIndex);
		session.State.Remove(S_Error);

		return WorkflowStepResult.Stay(new Dictionary<string, string>
		{
			[S_Mode] = "menu"
		});
	}

	private static bool LooksLikeCallbackText(string text)
		=> text.StartsWith("cmd:", StringComparison.OrdinalIgnoreCase)
		   || text.StartsWith("set:", StringComparison.OrdinalIgnoreCase)
		   || text.StartsWith("nav:", StringComparison.OrdinalIgnoreCase);

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
}
