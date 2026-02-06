using MissAlise.Wizards.Abstractions;

namespace MissAlise.Wizards.Descriptors;

public sealed class WizardDescriptorBuilder
{
    private readonly Dictionary<WizardStepId, Type> _steps = new();

    private WizardId? _id;
    private WizardStepId? _start;
    private WizardScope _scope = WizardScope.Private;

    public WizardDescriptorBuilder WithId(WizardId id)
    {
        _id = id;
        return this;
    }

    public WizardDescriptorBuilder WithScope(WizardScope scope)
    {
        _scope = scope;
        return this;
    }

    public WizardDescriptorBuilder StartWith<TStep>()
        where TStep : IWizardStep
    {
        _start = WizardStepId.From<TStep>();
        AddStep<TStep>();
        return this;
    }

    public WizardDescriptorBuilder AddStep<TStep>()
        where TStep : IWizardStep
    {
        _steps[WizardStepId.From<TStep>()] = typeof(TStep);
        return this;
    }

    public WizardDescriptor Build()
    {
        if (_id is null)
            throw new InvalidOperationException("WizardId is not set");

        if (_start is null)
            throw new InvalidOperationException("Start step is not set");

        return new WizardDescriptor
        {
            Id = _id.Value,
            StartStep = _start.Value,
            Steps = new Dictionary<WizardStepId, Type>(_steps),
            Scope = _scope
        };
    }
}
