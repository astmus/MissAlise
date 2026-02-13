using System;

namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Утилитарные фабрики для создания meta-значений (ValueBase) в одном месте.
/// Это упрощает BotBuilder и даёт единый стиль для будущих типов (Directory, Page, DateTime и т.д.).
/// </summary>
public static class ValueFactory
{
	public static StepValue<TimeSpan> TimeSpanSpinner(
		TimeSpan @default,
		TimeSpan? min,
		TimeSpan? max,
		TimeSpan step,
		int[]? multipliers = null,
		Func<TimeSpan, string>? formatter = null)
	{
		return new StepValue<TimeSpan>(@default)
		{
			Min = min,
			Max = max,
			StepBase = step,
			Multipliers = (multipliers is null || multipliers.Length == 0) ? new[] { 1 } : multipliers,
			Add = static (a, b) => a + b,
			Sub = static (a, b) => a - b,
			Formatter = formatter,
		};
	}
}
