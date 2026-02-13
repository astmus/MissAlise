using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Реестр визуальных групп для bool (тексты True/False).
/// </summary>
public static class BoolVisualGroups
{
	private static readonly Dictionary<string, (string TrueText, string FalseText)> _groups
		= new(StringComparer.OrdinalIgnoreCase)
		{
			["yesno"] = ("Да", "Нет"),
			["onoff"] = ("Вкл", "Выкл"),
			["active"] = ("Активно", "Неактивно"),
			["allow"] = ("Разрешить", "Запретить"),
		};

	public static (string TrueText, string FalseText) Get(string? groupKey)
	{
		var key = string.IsNullOrWhiteSpace(groupKey) ? "yesno" : groupKey;
		return _groups.TryGetValue(key!, out var v) ? v : _groups["yesno"];
	}

	public static void Register(string groupKey, string trueText, string falseText)
	{
		ArgumentNullException.ThrowIfNull(groupKey);
		_groups[groupKey] = (trueText, falseText);
	}
}
