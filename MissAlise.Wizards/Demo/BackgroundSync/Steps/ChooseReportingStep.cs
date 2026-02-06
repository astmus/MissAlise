using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Runtime;
using MissAlise.Wizards.Steps;

namespace MissAlise.Wizards.Demo.BackgroundSync.Steps;

public sealed class ChooseReportingStep : IWizardStep
{
    public Task<WizardStepResult> ExecuteAsync(
        WizardContext context,
        WizardSession session,
        WizardInput input,
        CancellationToken cancellationToken)
    {
        // Callbacks:
        //  bs:report:on
        //  bs:report:off
        if (input.Kind == WizardInputKind.Callback && input.Payload is not null)
        {
            if (input.Payload == "bs:report:on")
                session.Set(BackgroundSyncState.Reporting, "true");
            else if (input.Payload == "bs:report:off")
                session.Set(BackgroundSyncState.Reporting, "false");

            if (!string.IsNullOrWhiteSpace(session.Get(BackgroundSyncState.Reporting)))
            {
                // default active = true when created
                if (session.Get(BackgroundSyncState.Active) is null)
                    session.Set(BackgroundSyncState.Active, "true");

                return Task.FromResult(WizardStepResult.Next<ConfirmAndToggleStep>());
            }
        }

        return Task.FromResult(WizardStepResult.Stay());
    }
}
