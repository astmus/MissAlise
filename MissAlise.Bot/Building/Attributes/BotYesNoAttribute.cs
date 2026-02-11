using MissAlise.Workflow.Demo.BackgroundSync;

namespace MissAlise.TelegramBot.Building.Attributes;

/// <summary>Для параметра типа bool: отображать и вводить как Yes/No (допустимые значения "Yes", "No").</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class BotYesNoAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class BotActionAttribute(string action, int row) : Attribute
{
	public string Action { get; } = action;
	public int Row { get; } = row;
}
