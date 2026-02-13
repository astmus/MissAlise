namespace MissAlise.TelegramBot.Building.Attributes;

/// <summary>
/// Диапазон допустимых значений для числового параметра.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class BotRangeAttribute : Attribute
{
	public BotRangeAttribute(object min, object max)
	{
		Min = min;
		Max = max;
	}

	public object Min { get; }
	public object Max { get; }
}
