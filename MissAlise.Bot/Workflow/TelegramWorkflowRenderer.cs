using MissAlise.Application.Interfaces;
using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Workflow;

internal sealed class TelegramWorkflowRenderer : IWorkflowResultRenderer
{
    private readonly Bot _bot;
    private readonly IHandleContext _context;
    private readonly IWorkflowResultVisitor? _visitor;

    public TelegramWorkflowRenderer(Bot bot, IHandleContext context, IWorkflowResultVisitor? visitor = null)
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
        var chat = update?.GetCurrentChat() ?? _context.Get<Chat>();
        
        return WorkflowPresentationSender.SendAsync(_bot.ApiClient, chat, result.Presentation, cancel);
    }
}
