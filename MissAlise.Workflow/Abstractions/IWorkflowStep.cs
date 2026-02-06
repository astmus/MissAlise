using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow;

public interface IWorkflowStep
{
    Task<WorkflowStepResult> ExecuteAsync(
        WorkflowContext context,
        WorkflowSession session,
        WorkflowInput input,
        CancellationToken cancellationToken);
}
