namespace MissAlise.Wizards.Runtime;

public enum WizardInputKind
{
    None = 0,
    Command = 1,
    Callback = 2,
    Text = 3
}

public sealed class WizardInput
{
    public static readonly WizardInput Empty = new() { Kind = WizardInputKind.None };

    public required WizardInputKind Kind { get; init; }

    public string? Text { get; init; }

    /// <summary>Transport-specific payload (e.g., callback data in Telegram).</summary>
    public string? Payload { get; init; }

    public override string ToString() => $"{Kind}: {(Payload ?? Text ?? "<empty>")}";
}
