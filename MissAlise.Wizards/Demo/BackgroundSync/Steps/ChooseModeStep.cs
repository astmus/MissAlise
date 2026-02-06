using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Descriptors;
using MissAlise.Wizards.Runtime;
using MissAlise.Wizards.Steps;

namespace MissAlise.Wizards.Demo.BackgroundSync.Steps;

public sealed class ChooseModeStep : IWizardStep
{
    public static readonly WizardStepId Id = WizardStepId.From<ChooseModeStep>();

    public Task<WizardStepResult> ExecuteAsync(
        WizardContext context,
        WizardSession session,
        WizardInput input,
        CancellationToken cancellationToken)
    {
        // Expected callbacks:
        //  bs:mode:full
        //  bs:mode:diff
        //  bs:force:on
        //  bs:force:off
        if (input.Kind == WizardInputKind.Callback && input.Payload is not null)
        {
            if (input.Payload == "bs:mode:full")
                session.Set(BackgroundSyncState.Mode, "full");
            else if (input.Payload == "bs:mode:diff")
                session.Set(BackgroundSyncState.Mode, "diff");
            else if (input.Payload == "bs:force:on")
                session.Set(BackgroundSyncState.Force, "true");
            else if (input.Payload == "bs:force:off")
                session.Set(BackgroundSyncState.Force, "false");

            // proceed only after mode chosen
            var mode = session.Get(BackgroundSyncState.Mode);
            if (!string.IsNullOrWhiteSpace(mode))
                return Task.FromResult(WizardStepResult.Next<ChooseFolderStep>());
        }

        return Task.FromResult(WizardStepResult.Stay());
    }
}
