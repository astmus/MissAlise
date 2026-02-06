using MediatR;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Workflow;

namespace MissAlise.TelegramBot.Workflow;

/// <summary>
/// Publishes commands produced by workflow steps to MediatR (ICommand handlers).
/// </summary>
public sealed class MediatRWorkflowCommandSink : IWorkflowCommandSink
{
    private readonly IMediator _mediator;

    public MediatRWorkflowCommandSink(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task PublishAsync(object command, WorkflowContext context, CancellationToken cancellationToken)
    {
        if (command is IRequest<Result> request)
            await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }
}
