using System;
using System.Collections.Generic;
using System.Numerics;

namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Значение числового параметра с диапазоном и шагами изменения.
/// 
/// Поддерживает int/long/double/decimal и другие типы, реализующие <see cref="INumber{TSelf}"/> и <see cref="IParsable{TSelf}"/>.
/// </summary>
public sealed record NumericValue<T> : Value<T>
	where T : struct, INumber<T>, IParsable<T>
{
	public NumericValue(T? defaultValue = default) : base(defaultValue is { } v ? v : default) { }

	/// <summary>Минимально допустимое значение (включительно).</summary>
	public T? Min { get; init; }
	/// <summary>Максимально допустимое значение (включительно).</summary>
	public T? Max { get; init; }

	/// <summary>
	/// Базовый "шаг" изменения (например 1). Ускорения строятся как StepBase * multiplier.
	/// </summary>
	public T StepBase { get; init; } = T.One;

	/// <summary>
	/// Множители для "ускорения" рядом с базовыми кнопками (например 10, 20).
	/// Итоговый шаг: StepBase * multiplier.
	/// </summary>
	public IReadOnlyList<int> Multipliers { get; init; } = Array.Empty<int>();

	public T Clamp(T value)
	{
		if (Min is { } min && value < min) return min;
		if (Max is { } max && value > max) return max;
		return value;
	}

	public T GetDefaultOr(T fallback)
	{
		T? v = Default as T?;
		return v.HasValue ? v.Value : fallback;
	}

	/// <summary>
	/// Посчитать следующее значение на основе текущего и (baseStep * multiplier).
	/// direction: -1 / +1.
	/// </summary>
	public T Next(T current, int direction, int multiplier = 1)
	{
		var step = StepBase * T.CreateChecked(multiplier);
		var delta = direction < 0 ? -step : step;
		return Clamp(current + delta);
	}
}
