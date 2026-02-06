namespace MissAlise.Workflow;

public interface IWorkflowCommandSink
{
    Task PublishAsync(object command, WorkflowContext context, CancellationToken cancellationToken);
}
