namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Описание bool-параметра с указанием визуальной группы (Да/Нет, Вкл/Выкл, ...).
/// </summary>
public sealed record BoolValue : Value<bool>
{
	public BoolValue(bool? defaultValue = default) : base(defaultValue ?? default) { }

	/// <summary>
	/// Ключ визуальной группы. См. <see cref="BoolVisualGroups"/>.
	/// </summary>
	public string? VisualGroupKey { get; init; }
}
