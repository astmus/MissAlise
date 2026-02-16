using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Универсальное описание значения параметра. Тип T должен поддерживать парсинг (IParsable).
/// </summary>
public record Value<T> : ValueBase
	where T : IParsable<T>
{
	public Value(T? defaultValue = default)
	{
		Default = defaultValue ?? default;
	}

	public override Type ValueType => typeof(T);

	/// <summary>
	/// Список допустимых значений ("выбор из набора").
	/// Если задан — UI может показывать кнопки/inline.
	/// </summary>
	public IReadOnlyList<T>? AllowedValues { get; init; }
}
