using Microsoft.Extensions.DependencyInjection;
using MissAlise.Workflow;
using MissAlise.Workflow.Registry;

namespace MissAlise.Workflow;

public sealed class WorkflowCoordinator : IWorkflowCoordinator
{
	private readonly WorkflowRegistry _registry;
	private readonly IWorkflowStateStore _stateStore;
	private readonly IServiceProvider _services;
	private readonly IWorkflowPresenter _presenter;
	private readonly IWorkflowCommandSink? _commandSink;

	public WorkflowCoordinator(
		WorkflowRegistry registry,
		IWorkflowStateStore stateStore,
		IServiceProvider services,
		IWorkflowPresenter presenter,
		IWorkflowCommandSink? commandSink = null)
	{
		_registry = registry;
		_stateStore = stateStore;
		_services = services;
		_presenter = presenter;
		_commandSink = commandSink;
	}

	public async Task<WorkflowRequest> HandleAsync(
		WorkflowRequest request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var wasExpired = await _stateStore.WasExpiredAsync(request.Context, cancellationToken).ConfigureAwait(false);

		if (wasExpired)
		{
			await _stateStore.ClearActiveAsync(request.Context, cancellationToken);
			request.Result = WorkflowHandleResult.Expired();
			return request;
		}

		var session = await _stateStore.LoadOrCreateAsync(request.Context, cancellationToken);
		request.Session = session;

		var descriptor = _registry.Get(session.WorkflowId);

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

		if (session.IsCompleted)
		{
			await _stateStore.DeleteBySessionIdAsync(session.SessionId, cancellationToken);
			await _stateStore.ClearActiveAsync(request.Context, cancellationToken);
		}

		var presentation = _presenter.Present(session, request.Context);

		request.Result = new WorkflowHandleResult
		{
			Presentation = presentation,
			ProducedResults = produced,
			Context = request.Context
		};

		return request;
	}
}
