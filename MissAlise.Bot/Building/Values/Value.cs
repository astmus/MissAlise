using System;
using System.Collections.Generic;
using System.Globalization;

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

	/// <summary>Парсинг текстового значения в тип T в инвариантной культуре.</summary>
	public virtual bool TryParseRaw(string raw, out T value)
		=> T.TryParse(raw, CultureInfo.InvariantCulture, out value);

	/// <summary>Форматирование значения T в текст для хранения в state/CLI.</summary>
	public virtual string ToRaw(T value)
		=> value is IFormattable formattable
			? formattable.ToString(null, CultureInfo.InvariantCulture)
			: value.ToString() ?? string.Empty;

	/// <summary>Возвращает default из метаданных либо fallback.</summary>
	public virtual T GetDefaultOr(T fallback)
		=> Default is T v ? v : fallback;

	/// <summary>
	/// Вычисляет следующее значение по текущему и дельте.
	/// Переопределяется в конкретных ValueBase-типах (NumericValue/StepValue).
	/// </summary>
	public virtual T Next(T current, T delta)
		=> current;
}
