using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Значение-"спиннер" для типов, где инкремент/декремент задаётся поведением,
/// а не generic-math интерфейсами (TimeSpan, DateTime+TimeSpan, Page, Directory и т.д.).
///
/// Ключевая идея: арифметика/переходы задаются делегатами <see cref="Add"/>/<see cref="Sub"/>,
/// поэтому тип T НЕ обязан реализовывать IAdditionOperators/ISubtractionOperators.
/// </summary>
public sealed record StepValue<T> : Value<T>
	where T : struct, IComparable<T>, IParsable<T>
{
	public StepValue(T? defaultValue = default) : base(defaultValue is { } v ? v : default) { }

	/// <summary>Минимально допустимое значение (опционально).</summary>
	public T? Min { get; init; }
	/// <summary>Максимально допустимое значение (опционально).</summary>
	public T? Max { get; init; }

	/// <summary>
	/// Базовый шаг изменения (например, 1 секунда или 1 страница).
	/// </summary>
	public T StepBase { get; init; } = default;

	/// <summary>
	/// Множители для ускорения (например 10, 20 -> StepBase * multiplier).
	/// </summary>
	public IReadOnlyList<int> Multipliers { get; init; } = Array.Empty<int>();

	/// <summary>
	/// Функция "прибавить" (current + delta).
	/// Для TimeSpan: (a,b) => a + b.
	/// </summary>
	public Func<T, T, T>? Add { get; init; }

	/// <summary>
	/// Функция "вычесть" (current - delta).
	/// Для TimeSpan: (a,b) => a - b.
	/// </summary>
	public Func<T, T, T>? Sub { get; init; }

	/// <summary>
	/// Необязательное форматирование значения (для UI).
	/// </summary>
	public Func<T, string>? Formatter { get; init; }

	public string Format(T value)
		=> Formatter?.Invoke(value) ?? value.ToString() ?? string.Empty;

	public T Clamp(T value)
	{
		if (Min is { } min && value.CompareTo(min) < 0) return min;
		if (Max is { } max && value.CompareTo(max) > 0) return max;
		return value;
	}

	/// <summary>
	/// Рассчитать следующее значение на основе текущего и delta.
	/// Обычно используется в сценарии, где delta уже содержит знак (например TimeSpan).
	/// </summary>
	public override T Next(T current, T delta)
	{
		var add = Add;
		if (add is null)
			throw new InvalidOperationException($"{nameof(StepValue<T>)} for {typeof(T).Name} requires {nameof(Add)} to be set.");
		return Clamp(add(current, delta));
	}

	/// <summary>
	/// Рассчитать следующее значение.
	/// direction: -1 (назад) или +1 (вперёд)
	/// multiplier: 1 (базовый шаг) или множитель ускорения.
	/// </summary>
	public T Next(T current, int direction, int multiplier = 1)
	{
		var add = Add;
		var sub = Sub;
		if (add is null || sub is null)
			throw new InvalidOperationException($"{nameof(StepValue<T>)} for {typeof(T).Name} requires {nameof(Add)} and {nameof(Sub)} to be set.");

		var step = StepBase;
		var m = Math.Max(1, Math.Abs(multiplier));

		var result = current;
		for (var i = 0; i < m; i++)
			result = direction < 0 ? sub(result, step) : add(result, step);

		return Clamp(result);
	}
}
