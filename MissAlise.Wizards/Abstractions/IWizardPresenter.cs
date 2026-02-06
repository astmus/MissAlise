namespace MissAlise.Wizards.Abstractions;

public interface IWizardPresenter
{
    WizardStepPresentation Present(WizardSession session, WizardContext context);
}
