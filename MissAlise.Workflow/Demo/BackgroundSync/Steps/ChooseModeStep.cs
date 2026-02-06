using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow.Demo.BackgroundSync.Steps;

public sealed class ChooseModeStep : IWorkflowStep
{
    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowContext context,
        WorkflowSession session,
        WorkflowInput input,
        CancellationToken cancellationToken)
    {
        if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
        {
            if (input.Payload == "bs:mode:full") session.Set(BackgroundSyncState.Mode, "full");
            else if (input.Payload == "bs:mode:diff") session.Set(BackgroundSyncState.Mode, "diff");
            else if (input.Payload == "bs:force:on") session.Set(BackgroundSyncState.Force, "true");
            else if (input.Payload == "bs:force:off") session.Set(BackgroundSyncState.Force, "false");

            var mode = session.Get(BackgroundSyncState.Mode);
            if (!string.IsNullOrWhiteSpace(mode))
                return Task.FromResult(WorkflowStepResult.Next<ChooseFolderStep>());
        }

        return Task.FromResult(WorkflowStepResult.Stay());
    }
}
