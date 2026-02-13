namespace MissAlise.TelegramBot.Building.Attributes;

/// <summary>
/// Настройка визуальной группы для bool-параметра в wizard.
/// Пример: <c>[BotBoolStateGroup("yesno")]</c> или <c>[BotBoolStateGroup("onoff")]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class BotBoolStateGroupAttribute : Attribute
{
	public string GroupKey { get; }
	public uint Row { get; }

	public BotBoolStateGroupAttribute(string groupKey, uint row = 0)
	{
		GroupKey = groupKey;
		Row = row;
	}
}
