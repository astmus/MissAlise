namespace MissAlise.Wizards.Abstractions;

public interface IWizardStateStore
{
    Task<WizardSession> LoadOrCreateAsync(WizardContext context, CancellationToken cancellationToken);

    Task SaveAsync(WizardSession session, CancellationToken cancellationToken);
}
