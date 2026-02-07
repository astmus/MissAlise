using MissAlise.Workflow.PropertyEditing;
using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow.Demo.BackgroundSync.Steps;

public sealed class ChoosePeriodicityStep : IWorkflowStep
{
    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowContext context,
        WorkflowSession session,
        WorkflowInput input,
        CancellationToken cancellationToken)
    {
        var definition = new PropertyEditDefinition
        {
            Title = "Background Sync: периодичность",
            ValueLabel = "Period (sec)",
            StateKey = BackgroundSyncState.PeriodSeconds,
            Mode = PropertyEditMode.Increment,
            Step = 60,
            Min = 60,
            Max = 86400,
            ShowAccept = true,
            AcceptText = "Accept"
        };

        var editResult = PropertyEditHandler.TryHandle(input, session, definition, "prop");
        if (editResult.Handled)
        {
            if (editResult.Accepted)
                return Task.FromResult(WorkflowStepResult.Next<ChooseReportingStep>());

            return Task.FromResult(WorkflowStepResult.Stay());
        }

        if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null &&
            input.Payload.StartsWith("bs:period:", StringComparison.OrdinalIgnoreCase))
        {
            var raw = input.Payload["bs:period:".Length..];
            if (int.TryParse(raw, out var sec) && sec > 0)
            {
                session.Set(BackgroundSyncState.PeriodSeconds, sec.ToString());
                return Task.FromResult(WorkflowStepResult.Next<ChooseReportingStep>());
            }
        }

        if (input.Kind == WorkflowInputKind.Text && int.TryParse(input.Text?.Trim(), out var sec2) && sec2 > 0)
        {
            session.Set(BackgroundSyncState.PeriodSeconds, sec2.ToString());
            return Task.FromResult(WorkflowStepResult.Next<ChooseReportingStep>());
        }

        return Task.FromResult(WorkflowStepResult.Stay());
    }
}
