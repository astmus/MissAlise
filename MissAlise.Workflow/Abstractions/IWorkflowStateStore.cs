namespace MissAlise.Workflow;

public interface IWorkflowStateStore
{
	Task<bool> WasExpiredAsync(WorkflowContext context, CancellationToken ct);
	Task<WorkflowSession> LoadOrCreateAsync(WorkflowContext context, CancellationToken cancellationToken);
	Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken);

	Task DeleteBySessionIdAsync(string sessionId, CancellationToken cancellationToken);

	Task ClearActiveAsync(WorkflowContext context, CancellationToken cancellationToken);
}
