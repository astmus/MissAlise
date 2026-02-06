using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Runtime;
using MissAlise.Wizards.Steps;

namespace MissAlise.Wizards.Demo.BackgroundSync.Steps;

public sealed class ChooseFolderStep : IWizardStep
{
    public Task<WizardStepResult> ExecuteAsync(
        WizardContext context,
        WizardSession session,
        WizardInput input,
        CancellationToken cancellationToken)
    {
        // Expected:
        //  Text message like:  server:/photos client:c:\data\photos
        // or callback presets:
        //  bs:folder:preset:1
        if (input.Kind == WizardInputKind.Text && !string.IsNullOrWhiteSpace(input.Text))
        {
            // very minimal parser for demo; replace with proper UI later
            var text = input.Text.Trim();
            // server:/path client:/path
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                if (p.StartsWith("server:", StringComparison.OrdinalIgnoreCase))
                    session.Set(BackgroundSyncState.ServerFolder, p["server:".Length..]);
                if (p.StartsWith("client:", StringComparison.OrdinalIgnoreCase))
                    session.Set(BackgroundSyncState.ClientFolder, p["client:".Length..]);
            }

            if (!string.IsNullOrWhiteSpace(session.Get(BackgroundSyncState.ServerFolder)) &&
                !string.IsNullOrWhiteSpace(session.Get(BackgroundSyncState.ClientFolder)))
            {
                return Task.FromResult(WizardStepResult.Next<ChoosePeriodicityStep>());
            }
        }

        if (input.Kind == WizardInputKind.Callback && input.Payload == "bs:folder:preset:1")
        {
            session.Set(BackgroundSyncState.ServerFolder, "/incoming");
            session.Set(BackgroundSyncState.ClientFolder, "C:\\MissAlise\\Incoming");
            return Task.FromResult(WizardStepResult.Next<ChoosePeriodicityStep>());
        }

        return Task.FromResult(WizardStepResult.Stay());
    }
}
