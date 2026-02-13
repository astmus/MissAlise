namespace MissAlise.TelegramBot.Building.Attributes;

/// <summary>
/// Настройка шага изменения числового/step параметра.
/// BaseStep — стандартный шаг (например 1), Multipliers — ускорения (например 10, 20).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class BotStepAttribute : Attribute
{
	public BotStepAttribute(object baseStep, params int[] multipliers)
	{
		BaseStep = baseStep;
		Multipliers = multipliers ?? Array.Empty<int>();
	}

	public object BaseStep { get; }
	public int[] Multipliers { get; }
}
