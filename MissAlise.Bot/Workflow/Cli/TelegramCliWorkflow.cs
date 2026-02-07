using MissAlise.TelegramBot.CommandLine;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Registry;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Registers the Telegram CLI workflow (command routing + wizard) in WorkflowRegistry.
/// </summary>
internal sealed class TelegramCliWorkflow
{
	public static readonly WorkflowId Id = new("telegram-cli");
	public static readonly WorkflowStepId CliStep = new("cli");

	public static void Register(WorkflowRegistry registry)
	{
		var stps = new Dictionary<WorkflowStepId, Type>();
		stps[CliStep] = typeof(TelegramCliStep);

		var d = new WorkflowDescriptor
		{
			Id = Id,
			StartStep = CliStep,
			Steps = stps,
		};
		registry.Register(d);
	}
}
