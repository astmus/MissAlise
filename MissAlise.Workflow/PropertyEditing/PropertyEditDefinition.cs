namespace MissAlise.Workflow.PropertyEditing;

public sealed class PropertyEditDefinition
{
    public required string Title { get; init; }
    public required string StateKey { get; init; }
    public string ValueLabel { get; init; } = "Текущее значение";
    public PropertyEditMode Mode { get; init; }

    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();

    public int Step { get; init; } = 1;
    public int Min { get; init; } = int.MinValue;
    public int Max { get; init; } = int.MaxValue;

    public bool ShowAccept { get; init; }
    public string AcceptText { get; init; } = "Accept";
    public string DecrementText { get; init; } = "-";
    public string IncrementText { get; init; } = "+";

    public Func<string?, string>? FormatValue { get; init; }
}
