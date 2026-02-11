namespace MissAlise.TelegramBot.Building.Attributes;

/// <summary>Допустимые значения параметра (кнопки выбора).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class BotValueChooseAttribute : Attribute
{
	public object? DefaultValue { get; }
	public object[] Values { get; }

	public BotValueChooseAttribute(object defaultValue = null, params object[] values)
	{
		DefaultValue = defaultValue;
		Values = values;
	}
}

// <summary>Overrides CLI option metadata when using <see cref="BotBuilder"/>.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class BotDescriptionAttribute : Attribute
{
	public BotDescriptionAttribute(string description = null)
	{
		Description = description;
	}

	public string Description { get; }
}
