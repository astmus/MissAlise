namespace MissAlise.Wizards.Descriptors;

public readonly record struct WizardId(string Value)
{
    public override string ToString() => Value;

    public static WizardId From(string value)
        => new(value);
}
