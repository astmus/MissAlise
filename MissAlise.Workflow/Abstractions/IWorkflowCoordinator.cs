namespace MissAlise.Workflow;

public interface IWorkflowCoordinator
{
    Task<WorkflowRequest> HandleAsync(WorkflowRequest request, CancellationToken cancellationToken = default);
}
