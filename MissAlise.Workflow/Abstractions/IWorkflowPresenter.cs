using MissAlise.Workflow.Presentation;

namespace MissAlise.Workflow;

public interface IWorkflowPresenter
{
    WorkflowPresentation Present(WorkflowSession session, WorkflowContext context);
}
