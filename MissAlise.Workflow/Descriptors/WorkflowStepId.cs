namespace MissAlise.Workflow.Descriptors;

public readonly record struct WorkflowStepId(string Value)
{
    public override string ToString() => Value;
    public static WorkflowStepId From(string value) => new(value);
    public static WorkflowStepId From<TStep>() => new(typeof(TStep).Name);
}
