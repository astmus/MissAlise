namespace MissAlise.Workflow;

public interface IWorkflowCoordinator
{
    Task<WorkflowHandleResult> HandleAsync(
        WorkflowRequest request,
        CancellationToken cancellationToken = default);
}
