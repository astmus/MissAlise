namespace MissAlise.Workflow.Descriptors;

public readonly record struct WorkflowId(string Value)
{
    public override string ToString() => Value;
    public static WorkflowId From(string value) => new(value);
}
