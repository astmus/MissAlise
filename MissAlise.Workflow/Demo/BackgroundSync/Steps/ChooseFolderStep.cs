using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow.Demo.BackgroundSync.Steps;

public sealed class ChooseFolderStep : IWorkflowStep
{
    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowContext context,
        WorkflowSession session,
        WorkflowInput input,
        CancellationToken cancellationToken)
    {
        if (input.Kind == WorkflowInputKind.Text && !string.IsNullOrWhiteSpace(input.Text))
        {
            var parts = input.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                if (p.StartsWith("server:", StringComparison.OrdinalIgnoreCase))
                    session.Set(BackgroundSyncState.ServerFolder, p["server:".Length..]);
                if (p.StartsWith("client:", StringComparison.OrdinalIgnoreCase))
                    session.Set(BackgroundSyncState.ClientFolder, p["client:".Length..]);
            }

            if (!string.IsNullOrWhiteSpace(session.Get(BackgroundSyncState.ServerFolder)) &&
                !string.IsNullOrWhiteSpace(session.Get(BackgroundSyncState.ClientFolder)))
                return Task.FromResult(WorkflowStepResult.Next<ChoosePeriodicityStep>());
        }

        if (input.Kind == WorkflowInputKind.Callback && input.Payload == "bs:folder:preset:1")
        {
            session.Set(BackgroundSyncState.ServerFolder, "/incoming");
            session.Set(BackgroundSyncState.ClientFolder, "C:\\MissAlise\\Incoming");
            return Task.FromResult(WorkflowStepResult.Next<ChoosePeriodicityStep>());
        }

        return Task.FromResult(WorkflowStepResult.Stay());
    }
}
