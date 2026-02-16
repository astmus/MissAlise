using MissAlise.Workflow.Demo.BackgroundSync;

namespace MissAlise.TelegramBot.Building.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class BotActionAttribute(string action, int row) : Attribute
{
	public string Action { get; } = action;
	public int Row { get; } = row;
}
