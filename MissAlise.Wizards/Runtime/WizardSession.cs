using MissAlise.Wizards.Descriptors;
using MissAlise.Wizards.Steps;

namespace MissAlise.Wizards.Runtime;

public sealed class WizardSession
{
    public required long ChatId { get; init; }
    public required long UserId { get; init; }

    public required WizardId WizardId { get; set; }
    public required WizardStepId CurrentStep { get; set; }

    /// <summary>Arbitrary wizard state (simple key/value). Keep it transport-agnostic.</summary>
    public Dictionary<string, string> State { get; } = new(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Get(string key) => State.TryGetValue(key, out var v) ? v : null;

    public void Set(string key, string value) => State[key] = value;

    public bool GetBool(string key, bool @default = false)
        => State.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : @default;

    public int GetInt(string key, int @default = 0)
        => State.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : @default;

    public void Apply(WizardStepResult result)
    {
        UpdatedAt = DateTimeOffset.UtcNow;

        if (result.NextStep is not null)
            CurrentStep = result.NextStep.Value;

        if (result.StateUpdates is not null)
        {
            foreach (var (k, v) in result.StateUpdates)
                State[k] = v;
        }
    }
}
