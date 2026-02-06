using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Demo.BackgroundSync.Steps;

namespace MissAlise.Workflow.Demo.BackgroundSync;

public static class BackgroundSyncWorkflowDescriptor
{
    public static readonly WorkflowId Id = WorkflowId.From("background-sync");

    public static WorkflowDescriptor Create()
        => new WorkflowDescriptorBuilder()
            .WithId(Id)
            .StartWith<ChooseModeStep>()
            .AddStep<ChooseFolderStep>()
            .AddStep<ChoosePeriodicityStep>()
            .AddStep<ChooseReportingStep>()
            .AddStep<ConfirmAndToggleStep>()
            .Build();
}
