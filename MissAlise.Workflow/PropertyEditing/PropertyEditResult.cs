namespace MissAlise.Workflow.PropertyEditing;

public readonly record struct PropertyEditResult(bool Handled, bool Accepted)
{
    public static PropertyEditResult Ignored() => new(false, false);
    public static PropertyEditResult Updated() => new(true, false);
    public static PropertyEditResult Accepted() => new(true, true);
}
