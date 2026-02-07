using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow.Demo.BackgroundSync.Steps;

public sealed class ChooseReportingStep : IWorkflowStep
{
    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowContext context,
        WorkflowSession session,
        WorkflowInput input,
        CancellationToken cancellationToken)
    {
        if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
        {
            if (input.Payload == "bs:report:on") session.Set(BackgroundSyncState.Reporting, "true");
            else if (input.Payload == "bs:report:off") session.Set(BackgroundSyncState.Reporting, "false");

            if (!string.IsNullOrWhiteSpace(session.Get(BackgroundSyncState.Reporting)))
            {
                if (session.Get(BackgroundSyncState.Active) is null)
                    session.Set(BackgroundSyncState.Active, "true");

                return Task.FromResult(WorkflowStepResult.Next<ConfirmAndToggleStep>());
            }
        }

        return Task.FromResult(WorkflowStepResult.Stay());
    }
}
