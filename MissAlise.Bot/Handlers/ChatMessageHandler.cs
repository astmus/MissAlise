using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers;

internal sealed class ChatMessageHandler : BotAsyncHandlerBase<Message>
{
	private readonly ILogger<ChatMessageHandler> _log;
	private readonly IWorkflowCoordinator _coordinator;
	private readonly IWorkflowResultRenderer _renderer;

	public ChatMessageHandler(
		IHandleContext ctx,
		ILogger<ChatMessageHandler> log,
		Bot bot,
		IWorkflowCoordinator coordinator,
		IWorkflowResultRenderer renderer) : base(bot, ctx)
	{
		_log = log;
		_coordinator = coordinator;
		_renderer = renderer;
	}

	protected override async Task HandleAsync(Message data, CancellationToken cancel)
	{
		if (data.From is null) return;

		var request = WorkflowRequestFactory.FromMessage(data, data.From.Id);
		var response = await _coordinator.HandleAsync(request, cancel).ConfigureAwait(false);

		await _renderer.RenderAsync(response.Result, cancel).ConfigureAwait(false);
	}
}
