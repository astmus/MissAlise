namespace MissAlise.Wizards.Runtime;

public sealed class WizardContext
{
    public required long ChatId { get; init; }
    public required long UserId { get; init; }

    public string? Locale { get; init; }

    /// <summary>Optional correlation id / update id from transport layer.</summary>
    public string? CorrelationId { get; init; }
}
