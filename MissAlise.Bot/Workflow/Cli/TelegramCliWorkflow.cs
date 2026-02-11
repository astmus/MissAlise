using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Registry;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Registers the Telegram CLI workflow (command routing + wizard) in WorkflowRegistry.
/// </summary>
internal sealed class TelegramCliWorkflow
{
	public static readonly WorkflowId Id = new("telegram-cli");
	public static readonly WorkflowStepId MenuStep = new("cli:menu");
	public static readonly WorkflowStepId WizardStep = new("cli:wizard");
	public static readonly WorkflowStepId CommandStep = new("cli:command");

	public static void Register(WorkflowRegistry registry)
	{
		var stps = new Dictionary<WorkflowStepId, Type>();
		stps[MenuStep] = typeof(TelegramCliMenuStep);
		stps[WizardStep] = typeof(TelegramCliWizardStep);
		stps[CommandStep] = typeof(TelegramCliCommandStep);

		var d = new WorkflowDescriptor
		{
			Id = Id,
			StartStep = MenuStep,
			Steps = stps,
		};
		registry.Register(d);
	}
}
