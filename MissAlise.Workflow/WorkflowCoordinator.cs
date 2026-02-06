using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Registry;

namespace MissAlise.Workflow;

public sealed class WorkflowCoordinator : IWorkflowCoordinator
{
    private readonly WorkflowRegistry _registry;
    private readonly IWorkflowStateStore _stateStore;
    private readonly IServiceProvider _services;
    private readonly IWorkflowPresenter _presenter;
    private readonly IWorkflowCommandSink? _commandSink;
    private readonly HashSet<WorkflowId> _disposeSessionAfterProduce;

    public WorkflowCoordinator(
        WorkflowRegistry registry,
        IWorkflowStateStore stateStore,
        IServiceProvider services,
        IWorkflowPresenter presenter,
        IWorkflowCommandSink? commandSink = null,
        IReadOnlySet<WorkflowId>? disposeSessionAfterProduce = null,
        IOptions<WorkflowCoordinatorOptions>? options = null)
    {
        _registry = registry;
        _stateStore = stateStore;
        _services = services;
        _presenter = presenter;
        _commandSink = commandSink;
        _disposeSessionAfterProduce = options?.Value?.DisposeSessionAfterProduce != null
            ? new HashSet<WorkflowId>(options.Value.DisposeSessionAfterProduce)
            : disposeSessionAfterProduce != null
                ? new HashSet<WorkflowId>(disposeSessionAfterProduce)
                : new HashSet<WorkflowId>();
    }

    public async Task<WorkflowHandleResult> HandleAsync(
        WorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _stateStore.LoadOrCreateAsync(request.Context, cancellationToken);

        var descriptor = _registry.Get(session.WorkflowId);

        // Safety: if step wasn't initialized correctly, fall back to StartStep.
        if (string.IsNullOrWhiteSpace(session.CurrentStep.Value))
            session.CurrentStep = descriptor.StartStep;

        if (!descriptor.Steps.TryGetValue(session.CurrentStep, out var stepType))
            throw new InvalidOperationException($"Workflow '{session.WorkflowId}' does not contain step '{session.CurrentStep}'.");

        var step = (IWorkflowStep)_services.GetRequiredService(stepType);

        var stepResult = await step.ExecuteAsync(request.Context, session, request.Input, cancellationToken);

        session.Apply(stepResult);
        await _stateStore.SaveAsync(session, cancellationToken);

        var produced = stepResult.ProducedCommands?.ToArray() ?? Array.Empty<object>();
        if (_commandSink is not null && produced.Length > 0)
        {
            foreach (var cmd in produced)
                await _commandSink.PublishAsync(cmd, request.Context, cancellationToken);
        }

        if (produced.Length > 0 && _disposeSessionAfterProduce.Contains(session.WorkflowId))
            await _stateStore.DeleteAsync(request.Context, cancellationToken);

        var presentation = _presenter.Present(session, request.Context);

        return new WorkflowHandleResult
        {
            Presentation = presentation,
            ProducedCommands = produced
        };
    }
}
