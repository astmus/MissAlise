namespace MissAlise.Wizards.Presentation;

public sealed class WizardButton
{
    public required string Text { get; init; }

    /// <summary>Opaque payload that transport layer will send back to us (e.g. Telegram callback data).</summary>
    public required string Payload { get; init; }
}
