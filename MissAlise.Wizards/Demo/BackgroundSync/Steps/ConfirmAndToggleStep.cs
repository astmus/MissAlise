using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Runtime;
using MissAlise.Wizards.Steps;

namespace MissAlise.Wizards.Demo.BackgroundSync.Steps;

public sealed class ConfirmAndToggleStep : IWizardStep
{
    public Task<WizardStepResult> ExecuteAsync(
        WizardContext context,
        WizardSession session,
        WizardInput input,
        CancellationToken cancellationToken)
    {
        // Callbacks:
        //  bs:confirm
        //  bs:toggle
        if (input.Kind == WizardInputKind.Callback && input.Payload is not null)
        {
            if (input.Payload == "bs:toggle")
            {
                var active = session.GetBool(BackgroundSyncState.Active, true);
                session.Set(BackgroundSyncState.Active, (!active).ToString().ToLowerInvariant());

                var cmd = Build(session);
                return Task.FromResult(WizardStepResult.Produce(cmd));
            }

            if (input.Payload == "bs:confirm")
            {
                var cmd = Build(session);
                return Task.FromResult(WizardStepResult.Produce(cmd));
            }
        }

        return Task.FromResult(WizardStepResult.Stay());
    }

    private static BackgroundSyncCommand Build(WizardSession session)
    {
        var mode = session.Get(BackgroundSyncState.Mode) == "diff"
            ? BackgroundSyncMode.Diff
            : BackgroundSyncMode.Full;

        var force = session.GetBool(BackgroundSyncState.Force, false);

        var periodSec = session.GetInt(BackgroundSyncState.PeriodSeconds, 300);
        var periodicity = TimeSpan.FromSeconds(periodSec);

        var serverFolder = session.Get(BackgroundSyncState.ServerFolder) ?? "/";
        var clientFolder = session.Get(BackgroundSyncState.ClientFolder) ?? "C:\\";

        var reporting = session.GetBool(BackgroundSyncState.Reporting, false);
        var active = session.GetBool(BackgroundSyncState.Active, true);

        return new BackgroundSyncCommand(mode, force, periodicity, serverFolder, clientFolder, reporting, active);
    }
}
