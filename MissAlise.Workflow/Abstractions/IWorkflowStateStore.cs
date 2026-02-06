namespace MissAlise.Workflow;

public interface IWorkflowStateStore
{
    /// <summary>Returns existing session or null if none (does not create).</summary>
    Task<WorkflowSession?> TryLoadAsync(WorkflowContext context, CancellationToken cancellationToken);

    /// <summary>Loads session or creates a new one with default workflow/step (e.g. for workflows that always have a session).</summary>
    Task<WorkflowSession> LoadOrCreateAsync(WorkflowContext context, CancellationToken cancellationToken);

    /// <summary>Creates and stores a new session (e.g. for parameter collection). Fails if session for context already exists.</summary>
    Task CreateAsync(WorkflowSession session, CancellationToken cancellationToken);

    Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken);

    /// <summary>Removes session for the given context (e.g. when parameter collection completes).</summary>
    Task DeleteAsync(WorkflowContext context, CancellationToken cancellationToken);
}
