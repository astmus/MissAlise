using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot;
using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Workflow;

public interface IWorkflowResultVisitor
{
    Task VisitAsync(WorkflowHandleResult result, CancellationToken cancellationToken = default);
}

public abstract class WorkflowResultVisitor : IWorkflowResultVisitor
{
    private readonly IHandleContext _context;

    protected WorkflowResultVisitor(IHandleContext context)
    {
        _context = context;
    }

    public virtual Task VisitAsync(WorkflowHandleResult result, CancellationToken cancellationToken = default)
    {
        var update = _context.Get<UpdateExt>() ?? _context.Get<Update>();
        if (update is null)
        {
            return Task.CompletedTask;
        }

        return update switch
        {
            { Message: not null } => VisitMessageAsync(update.Message, result, cancellationToken),
            { CallbackQuery: not null } => VisitCallbackQueryAsync(update.CallbackQuery, result, cancellationToken),
            { InlineQuery: not null } => VisitInlineQueryAsync(update.InlineQuery, result, cancellationToken),
            _ => VisitUnknownAsync(result, cancellationToken)
        };
    }

    protected virtual Task VisitMessageAsync(Message message, WorkflowHandleResult result, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected virtual Task VisitCallbackQueryAsync(CallbackQuery callbackQuery, WorkflowHandleResult result, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected virtual Task VisitInlineQueryAsync(InlineQuery inlineQuery, WorkflowHandleResult result, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected virtual Task VisitUnknownAsync(WorkflowHandleResult result, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
