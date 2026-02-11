using MissAlise.Application.Interfaces;
using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Workflow;

internal sealed class TelegramWorkflowRenderer : IWorkflowResultRenderer
{
	private readonly Bot _bot;
	private readonly IHandleContext _context;
	private readonly IWorkflowStateStore? _stateStore;
	private readonly IWorkflowResultVisitor? _visitor;

	public TelegramWorkflowRenderer(Bot bot, IHandleContext context, IWorkflowStateStore? stateStore = null, IWorkflowResultVisitor? visitor = null)
	{
		_bot = bot;
		_context = context;
		_stateStore = stateStore;
		_visitor = visitor;
	}

	public async Task RenderAsync(WorkflowHandleResult result, CancellationToken cancel)
	{
		var update = _context.Get<UpdateExt>() ?? _context.Get<Update>();
		var chat = update?.GetCurrentChat() ?? _context.Get<Chat>();

		if (chat is null) return;

		var chatId = chat.Id;

		if (result.Presentation == WorkflowPresentation.Empty)
			return;

		if (_visitor != null)
		{
			await _visitor.VisitAsync(result, cancel).ConfigureAwait(false);
			return;
		}

		//if (result.EditMessageId is { } editId)
		//{
		//	await WorkflowPresentationSender.EditAsync(_bot.ApiClient, chatId, editId, result.Presentation, cancel).ConfigureAwait(false);
		//	return;
		//}

		var sent = await WorkflowPresentationSender.SendAsync(_bot.ApiClient, chatId, result.Presentation, cancel).ConfigureAwait(false);
	}
}
