using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Runtime;
using MissAlise.Wizards.Steps;

namespace MissAlise.Wizards.Demo.BackgroundSync.Steps;

public sealed class ChoosePeriodicityStep : IWizardStep
{
    public Task<WizardStepResult> ExecuteAsync(
        WizardContext context,
        WizardSession session,
        WizardInput input,
        CancellationToken cancellationToken)
    {
        // Callbacks:
        //  bs:period:300  (seconds)
        //  bs:period:900
        //  bs:period:3600
        if (input.Kind == WizardInputKind.Callback && input.Payload is not null &&
            input.Payload.StartsWith("bs:period:", StringComparison.OrdinalIgnoreCase))
        {
            var raw = input.Payload["bs:period:".Length..];
            if (int.TryParse(raw, out var sec) && sec > 0)
            {
                session.Set(BackgroundSyncState.PeriodSeconds, sec.ToString());
                return Task.FromResult(WizardStepResult.Next<ChooseReportingStep>());
            }
        }

        if (input.Kind == WizardInputKind.Text && int.TryParse(input.Text?.Trim(), out var sec2) && sec2 > 0)
        {
            session.Set(BackgroundSyncState.PeriodSeconds, sec2.ToString());
            return Task.FromResult(WizardStepResult.Next<ChooseReportingStep>());
        }

        return Task.FromResult(WizardStepResult.Stay());
    }
}
