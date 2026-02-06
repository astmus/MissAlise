using MissAlise.Wizards.Descriptors;

namespace MissAlise.Wizards.Steps;

public sealed class WizardStepResult
{
    public WizardStepId? NextStep { get; init; }

    public IReadOnlyDictionary<string, string>? StateUpdates { get; init; }

    public IReadOnlyList<object>? ProducedCommands { get; init; }

    public static WizardStepResult Stay(IReadOnlyDictionary<string, string>? updates = null)
        => new() { StateUpdates = updates };

    public static WizardStepResult Next(WizardStepId next, IReadOnlyDictionary<string, string>? updates = null)
        => new() { NextStep = next, StateUpdates = updates };

    public static WizardStepResult Next<TStep>(IReadOnlyDictionary<string, string>? updates = null)
        => Next(WizardStepId.From<TStep>(), updates);

    public static WizardStepResult Produce(object command, WizardStepId? next = null, IReadOnlyDictionary<string, string>? updates = null)
        => new()
        {
            NextStep = next,
            StateUpdates = updates,
            ProducedCommands = new[] { command }
        };

    public static WizardStepResult ProduceMany(IEnumerable<object> commands, WizardStepId? next = null, IReadOnlyDictionary<string, string>? updates = null)
        => new()
        {
            NextStep = next,
            StateUpdates = updates,
            ProducedCommands = commands.ToArray()
        };
}
