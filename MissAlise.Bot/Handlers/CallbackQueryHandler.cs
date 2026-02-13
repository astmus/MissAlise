using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers
{
	internal class CallbackQueryHandler : BotAsyncHandlerBase<CallbackQuery>
	{		
		private readonly ILogger<CallbackQueryHandler> _log;
		private readonly IWorkflowCoordinator? _engine;
		private readonly IWorkflowStateStore? _workflowStore;
		private readonly IWorkflowResultRenderer? _workflowRenderer;

		public CallbackQueryHandler(
			Bot bot,
			IHandleContext ctx,
			ILogger<CallbackQueryHandler> log,
			IWorkflowCoordinator? engine = null,
			IWorkflowStateStore? workflowStore = null,
			IWorkflowResultRenderer? workflowRenderer = null) : base(bot, ctx)
		{
			_log = log;			
			_engine = engine;
			_workflowStore = workflowStore;
			_workflowRenderer = workflowRenderer;
		}

		protected override async Task BeforeHandleAsync(CallbackQuery data, CancellationToken cancel)
		{
			_ = _bot.ApiClient.AnswerCallbackQuery(data.Id, cancellationToken: cancel).ConfigureAwait(false);				
			await _bot.ApiClient.SendChatAction(Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancel).ConfigureAwait(false);
		}

		protected override async Task HandleAsync(CallbackQuery data, CancellationToken cancel)
		{
			DateTime start;
			_log.LogInformation("Start handle callback {Id} {now}", data.Id, (start = DateTime.UtcNow).ToLongTimeString());
			var chatId = data.Message?.Chat.Id;
			ArgumentNullException.ThrowIfNull(data.Message?.Chat.Id, nameof(chatId));

			var request = WorkflowRequestFactory.FromCallback(data, chatId.Value, data.From.Id);
			var response = await _engine.HandleAsync(request, cancel).ConfigureAwait(false);

			await _workflowRenderer.RenderAsync(response.Result, cancel).ConfigureAwait(false);

			_log.LogInformation("End handle callback {Id} {now}", data.Id, (DateTime.UtcNow - start));
		}
	}
}
