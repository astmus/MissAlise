using MissAlise.Application.Common;

namespace MissAlise.Workflow;

public interface IWorkflowCommandSink
{
    Task<Result> PublishAsync(object command, WorkflowContext context, CancellationToken cancellationToken);
}
