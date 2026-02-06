namespace MissAlise.Workflow.Descriptors;

public sealed class WorkflowDescriptorBuilder
{
    private readonly Dictionary<WorkflowStepId, Type> _steps = new();
    private WorkflowId? _id;
    private WorkflowStepId? _start;
    private WorkflowScope _scope = WorkflowScope.Private;

    public WorkflowDescriptorBuilder WithId(WorkflowId id) { _id = id; return this; }
    public WorkflowDescriptorBuilder WithScope(WorkflowScope scope) { _scope = scope; return this; }

    public WorkflowDescriptorBuilder StartWith<TStep>() where TStep : IWorkflowStep
    {
        _start = WorkflowStepId.From<TStep>();
        AddStep<TStep>();
        return this;
    }

    public WorkflowDescriptorBuilder AddStep<TStep>() where TStep : IWorkflowStep
    {
        _steps[WorkflowStepId.From<TStep>()] = typeof(TStep);
        return this;
    }

    public WorkflowDescriptor Build()
    {
        if (_id is null) throw new InvalidOperationException("WorkflowId is not set");
        if (_start is null) throw new InvalidOperationException("Start step is not set");

        return new WorkflowDescriptor
        {
            Id = _id.Value,
            StartStep = _start.Value,
            Steps = new Dictionary<WorkflowStepId, Type>(_steps),
            Scope = _scope
        };
    }
}
