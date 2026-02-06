using MissAlise.Wizards.Presentation;

namespace MissAlise.Wizards.Runtime;

public sealed class WizardHandleResult
{
    public required WizardStepPresentation Presentation { get; init; }

    /// <summary>Commands produced by the wizard flow (e.g., BackgroundSyncCommand).</summary>
    public IReadOnlyList<object> ProducedCommands { get; init; } = Array.Empty<object>();
}
