namespace MissAlise.TelegramBot.Building.Values;

/// <summary>Значение «выбор из списка» (не типизированное парсируемым типом).</summary>
public sealed record ChoiceValue : ValueBase
{
	public ChoiceValue(object? defaultValue = default)
	{
		Default = defaultValue;
	}

	public override Type ValueType => typeof(object);

	/// <summary>Список допустимых значений.</summary>
	public IReadOnlyList<object>? AllowedValues { get; init; }
}
