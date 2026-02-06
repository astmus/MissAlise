namespace MissAlise.Wizards.Presentation;

public sealed class WizardStepPresentation
{
    public required string Text { get; init; }

    public IReadOnlyList<WizardButton> Buttons { get; init; } = Array.Empty<WizardButton>();
}
