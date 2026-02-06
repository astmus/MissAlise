namespace MissAlise.Wizards.Abstractions;

public interface IWizardCoordinator
{
    Task<WizardHandleResult> HandleAsync(
        WizardRequest request,
        CancellationToken cancellationToken = default);
}
