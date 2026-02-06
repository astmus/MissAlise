namespace MissAlise.Workflow;

public enum WorkflowInputKind
{
    None = 0,
    Command = 1,
    Callback = 2,
    Text = 3
}

public sealed class WorkflowInput
{
    public static readonly WorkflowInput Empty = new() { Kind = WorkflowInputKind.None };

    public required WorkflowInputKind Kind { get; init; }
    public string? Text { get; init; }
    public string? Payload { get; init; }
}
