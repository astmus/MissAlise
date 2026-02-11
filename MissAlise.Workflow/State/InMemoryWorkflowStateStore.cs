using System;
using System.Collections.Generic;
using System.Linq;
using MissAlise.Workflow.Descriptors;

namespace MissAlise.Workflow.State;

public sealed class InMemoryWorkflowStateStore : IWorkflowStateStore
{
	private readonly object _lock = new();
	// Session storage by instance id.
	private readonly Dictionary<string, WorkflowSession> _sessionsById = new(StringComparer.OrdinalIgnoreCase);

	// "Active session" pointer per context (chat/user). This is an index, not the session itself.
	private readonly Dictionary<(long chatId, long userId), string> _activeByContext = new();

	private readonly WorkflowId _defaultWorkflowId;
	private readonly WorkflowStepId _defaultStartStep;

	public InMemoryWorkflowStateStore(WorkflowId defaultWorkflowId, WorkflowStepId defaultStartStep)
	{
		_defaultWorkflowId = defaultWorkflowId;
		_defaultStartStep = defaultStartStep;
	}

	public Task<WorkflowSession> LoadOrCreateAsync(WorkflowContext context, CancellationToken cancellationToken)
	{
		var key = (context.ChatId, context.UserId);

		lock (_lock)
		{
			if (_activeByContext.TryGetValue(key, out var sid)
				&& _sessionsById.TryGetValue(sid, out var existing))
			{
				return Task.FromResult(existing);
			}

			var session = new WorkflowSession
			{
				SessionId = Guid.NewGuid().ToString("N"),
				ChatId = context.ChatId,
				UserId = context.UserId,
				WorkflowId = _defaultWorkflowId,
				CurrentStep = _defaultStartStep,
			};

			_sessionsById[session.SessionId] = session;
			_activeByContext[key] = session.SessionId;
			return Task.FromResult(session);
		}
	}

	public Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken)
	{
		var key = (session.ChatId, session.UserId);
		lock (_lock)
		{
			_sessionsById[session.SessionId] = session;

			// Update the active pointer only for non-completed sessions.
			if (!session.IsCompleted)
				_activeByContext[key] = session.SessionId;
			else
				_activeByContext.Remove(key);
		}
		return Task.CompletedTask;
	}

	public Task DeleteBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
	{
		lock (_lock)
		{
			_sessionsById.Remove(sessionId);

			// Clean up any active pointers referencing this session.
			var toRemove = _activeByContext
				.Where(kvp => string.Equals(kvp.Value, sessionId, StringComparison.OrdinalIgnoreCase))
				.Select(kvp => kvp.Key)
				.ToArray();
			foreach (var k in toRemove)
				_activeByContext.Remove(k);
		}

		return Task.CompletedTask;
	}

	public Task ClearActiveAsync(WorkflowContext context, CancellationToken cancellationToken)
	{
		var key = (context.ChatId, context.UserId);
		lock (_lock) { _activeByContext.Remove(key); }
		return Task.CompletedTask;
	}

	public Task<bool> WasExpiredAsync(WorkflowContext context, CancellationToken ct) => throw new NotImplementedException();
}
