namespace MissAlise.Wizards.Descriptors;

public readonly record struct WizardStepId(string Value)
{
    public override string ToString() => Value;

    public static WizardStepId From(string value) => new(value);

    public static WizardStepId From<TStep>() => new(typeof(TStep).Name);
}
