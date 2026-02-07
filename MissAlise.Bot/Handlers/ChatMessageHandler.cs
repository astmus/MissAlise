using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers;

/// <summary>
/// Thin wrapper: Message -> WorkflowCoordinator -> TelegramWorkflowRenderer.
/// </summary>
internal sealed class ChatMessageHandler : BotAsyncHandlerBase<Message>
{
	private readonly IHandleContext _ctx;
	private readonly ILogger<ChatMessageHandler> _log;
	private readonly IWorkflowCoordinator _coordinator;
	private readonly IWorkflowResultRenderer _renderer;

	public ChatMessageHandler(
		IHandleContext ctx,
		ILogger<ChatMessageHandler> log,
		Bot bot,
		IWorkflowCoordinator coordinator,
		IWorkflowResultRenderer renderer) : base(bot)
	{
		_ctx = ctx;
		_log = log;
		_coordinator = coordinator;
		_renderer = renderer;
	}

	protected override async Task HandleAsync(Message data, CancellationToken cancel)
	{
		if (data.From is null) return;

		var request = WorkflowRequestFactory.FromMessage(data, data.From.Id);

		var result = await _coordinator.HandleAsync(request, cancel).ConfigureAwait(false);
		await _renderer.RenderAsync(result, cancel).ConfigureAwait(false);

	}
}
