namespace MissAlise.Wizards.Abstractions;

public interface IWizardStep
{
    Task<WizardStepResult> ExecuteAsync(
        WizardContext context,
        WizardSession session,
        WizardInput input,
        CancellationToken cancellationToken);
}
