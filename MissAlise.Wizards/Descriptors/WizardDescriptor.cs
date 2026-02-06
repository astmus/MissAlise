namespace MissAlise.Wizards.Descriptors;

public sealed class WizardDescriptor
{
    public required WizardId Id { get; init; }
    public required WizardStepId StartStep { get; init; }

    public required IReadOnlyDictionary<WizardStepId, Type> Steps { get; init; }

    public WizardScope Scope { get; init; } = WizardScope.Private;

    public override string ToString() => $"{Id} (start: {StartStep}, steps: {Steps.Count})";
}
