using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;

namespace MissAlise.Workflow.State;

public sealed class InMemoryWorkflowStateStore : IWorkflowStateStore
{
    private readonly object _lock = new();
    private readonly Dictionary<(long chatId, long userId), WorkflowSession> _sessions = new();

    private readonly WorkflowId _defaultWorkflowId;
    private readonly WorkflowStepId _defaultStartStep;

    public InMemoryWorkflowStateStore(WorkflowId defaultWorkflowId, WorkflowStepId defaultStartStep)
    {
        _defaultWorkflowId = defaultWorkflowId;
        _defaultStartStep = defaultStartStep;
    }

    public Task<WorkflowSession?> TryLoadAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        var key = (context.ChatId, context.UserId);
        lock (_lock)
        {
            return _sessions.TryGetValue(key, out var session)
                ? Task.FromResult<WorkflowSession?>(session)
                : Task.FromResult<WorkflowSession?>(null);
        }
    }

    public Task<WorkflowSession> LoadOrCreateAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        var key = (context.ChatId, context.UserId);

        lock (_lock)
        {
            if (_sessions.TryGetValue(key, out var session))
                return Task.FromResult(session);

            session = new WorkflowSession
            {
                ChatId = context.ChatId,
                UserId = context.UserId,
                WorkflowId = _defaultWorkflowId,
                CurrentStep = _defaultStartStep,
            };

            _sessions[key] = session;
            return Task.FromResult(session);
        }
    }

    public Task CreateAsync(WorkflowSession session, CancellationToken cancellationToken)
    {
        var key = (session.ChatId, session.UserId);
        lock (_lock)
        {
            if (_sessions.ContainsKey(key))
                throw new InvalidOperationException($"Session for chat {session.ChatId}, user {session.UserId} already exists.");
            _sessions[key] = session;
        }
        return Task.CompletedTask;
    }

    public Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken)
    {
        var key = (session.ChatId, session.UserId);
        lock (_lock) { _sessions[key] = session; }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        var key = (context.ChatId, context.UserId);
        lock (_lock) { _sessions.Remove(key); }
        return Task.CompletedTask;
    }
}
