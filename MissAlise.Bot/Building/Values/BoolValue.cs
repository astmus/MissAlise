namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Описание bool-параметра. Визуальная группа задаётся через <see cref="ValueBase.VisualStateKey"/> (см. <see cref="BoolVisualGroups"/>).
/// </summary>
public sealed record BoolValue : Value<bool>
{
	public BoolValue(bool? defaultValue = default) : base(defaultValue ?? default) { }
}
