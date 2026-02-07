using MissAlise.Application.Interfaces;
using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Workflow;

internal sealed class TelegramWorkflowRenderer : IWorkflowResultRenderer
{
    private readonly Bot _bot;
    private readonly IHandleContext _context;
    private readonly IWorkflowUpdateResultVisitor? _visitor;

    public TelegramWorkflowRenderer(Bot bot, IHandleContext context, IWorkflowUpdateResultVisitor? visitor = null)
    {
        _bot = bot;
        _context = context;
        _visitor = visitor;
    }

    public Task RenderAsync(WorkflowHandleResult result, CancellationToken cancel)
    {
        if (_visitor != null)
        {
            return _visitor.VisitAsync(result, cancel);
        }

        var update = _context.Get<UpdateExt>() ?? _context.Get<Update>();
        var chatId = update?.GetCurrentChat()?.Id ?? _context.Get<Chat>()?.Id ?? 0L;
        if (chatId == 0)
        {
            return Task.CompletedTask;
        }

        return WorkflowPresentationSender.SendAsync(_bot.ApiClient, chatId, result.Presentation, cancel);
    }
}
