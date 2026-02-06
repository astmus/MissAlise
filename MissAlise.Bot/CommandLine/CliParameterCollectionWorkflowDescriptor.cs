using MissAlise.Workflow.Descriptors;

namespace MissAlise.TelegramBot.CommandLine;

public static class CliParameterCollectionWorkflowDescriptor
{
	public static readonly WorkflowId Id = WorkflowId.From(CompositeWorkflowPresenter.CliParamsWorkflowId);

	public static WorkflowDescriptor Create()
		=> new WorkflowDescriptorBuilder()
			.WithId(Id)
			.StartWith<CollectParameterStep>()
			.Build();
}
