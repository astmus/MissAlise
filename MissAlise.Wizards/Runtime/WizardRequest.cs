namespace MissAlise.Wizards.Runtime;

public sealed class WizardRequest
{
    public required WizardContext Context { get; init; }
    public required WizardInput Input { get; init; }
}
