using MediatR;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.CommandLine;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers
{
	internal class CallbackQueryHandler : BotAsyncHandlerBase<CallbackQuery>
	{
		private readonly IHandleContext _ctx;
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
			IWorkflowResultRenderer? workflowRenderer = null) : base(bot)
		{
			_log = log;
			_ctx = ctx;
			_engine = engine;
			_workflowStore = workflowStore;
			_workflowRenderer = workflowRenderer;
		}


		protected override async Task HandleAsync(CallbackQuery data, CancellationToken cancel)
		{
			var chatId = data.Message?.Chat.Id;
			if (chatId is null) return;


			var request = WorkflowRequestFactory.FromCallback(data, chatId.Value, data.From.Id);
			var result = await _engine.HandleAsync(request, cancel).ConfigureAwait(false);
			await _workflowRenderer.RenderAsync(chatId.Value, result.Presentation, cancel).ConfigureAwait(false);

			try { await _bot.ApiClient.AnswerCallbackQueryAsync(data.Id, cancellationToken: cancel).ConfigureAwait(false); } catch { /* ignore */ }
		}
	}
} 
