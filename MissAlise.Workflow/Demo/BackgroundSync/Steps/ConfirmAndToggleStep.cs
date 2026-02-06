using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow.Demo.BackgroundSync.Steps;

public sealed class ConfirmAndToggleStep : IWorkflowStep
{
    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowContext context,
        WorkflowSession session,
        WorkflowInput input,
        CancellationToken cancellationToken)
    {
        if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
        {
            if (input.Payload == "bs:toggle")
            {
                var active = session.GetBool(BackgroundSyncState.Active, true);
                session.Set(BackgroundSyncState.Active, (!active).ToString().ToLowerInvariant());
                return Task.FromResult(WorkflowStepResult.Produce(Build(session)));
            }

            if (input.Payload == "bs:confirm")
			{
				return Task.FromResult(WorkflowStepResult.Produce(Build(session)));
			}
		}

        return Task.FromResult(WorkflowStepResult.Stay());
    }

    private static BackgroundSyncCommand Build(WorkflowSession s)
    {
        var mode = s.Get(BackgroundSyncState.Mode) == "diff" ? BackgroundSyncMode.Diff : BackgroundSyncMode.Full;
        var force = s.GetBool(BackgroundSyncState.Force, false);
        var sec = s.GetInt(BackgroundSyncState.PeriodSeconds, 300);
        var server = s.Get(BackgroundSyncState.ServerFolder) ?? "/";
        var client = s.Get(BackgroundSyncState.ClientFolder) ?? "C:\\";
        var reporting = s.GetBool(BackgroundSyncState.Reporting, false);
        var active = s.GetBool(BackgroundSyncState.Active, true);

        return new BackgroundSyncCommand(mode, force, TimeSpan.FromSeconds(sec), server, client, reporting, active);
    }
}
