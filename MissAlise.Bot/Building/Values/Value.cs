using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Универсальное описание значения параметра.
/// </summary>
/// <summary>Значение по умолчанию, если пользователь ещё не ввёл параметр (позиционный параметр DefaultValue).</summary>
public record Value<T> : ValueBase
{
	public Value(T? DefaultValue = default)
	{
		Default = DefaultValue ?? default;	
	}

	public override Type ValueType => typeof(T);

	/// <summary>
	/// Список допустимых значений ("выбор из набора").
	/// Если задан — UI может показывать кнопки/inline.
	/// </summary>
	public IReadOnlyList<T>? AllowedValues { get; init; }
}
