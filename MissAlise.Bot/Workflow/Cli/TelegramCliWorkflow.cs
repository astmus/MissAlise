using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Registry;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Registers the Telegram CLI workflow (command routing + wizard) in WorkflowRegistry.
/// </summary>
internal sealed class TelegramCliWorkflow
{
	public static readonly WorkflowId Id = new("telegram-cli");
	public static readonly WorkflowStepId MenuStep = WorkflowStepId.From<TelegramCliMenuStep>();
	public static readonly WorkflowStepId WizardStep = WorkflowStepId.From<TelegramCliWizardStep>();
	public static readonly WorkflowStepId CommandStep = WorkflowStepId.From<TelegramCliCommandStep>();

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
