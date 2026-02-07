using MediatR;
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
		private readonly IMediator _mm;
		private readonly Bot _bot;
		private readonly IWorkflowCoordinator? _engine;
		private readonly IBotCommandLineParser? _cliParser;
		private readonly IBotCommandFactory? _cliFactory;
		private readonly IWorkflowStateStore? _workflowStore;
		private readonly IWorkflowResultRenderer? _workflowRenderer;

		public CallbackQueryHandler(
			IHandleContext ctx,
			ILogger<CallbackQueryHandler> log,
			IMediator mm,
			Bot bot,
			IWorkflowCoordinator? engine = null,
			IBotCommandLineParser? cliParser = null,
			IBotCommandFactory? cliFactory = null,
			IWorkflowStateStore? workflowStore = null,
			IWorkflowResultRenderer? workflowRenderer = null) : base(bot)
		{
			_log = log;
			_mm = mm;
			_ctx = ctx;
			_bot = bot;
			_engine = engine;
			_cliParser = cliParser;
			_cliFactory = cliFactory;
			_workflowStore = workflowStore;
			_workflowRenderer = workflowRenderer;
		}

		protected override async Task HandleAsync(CallbackQuery data, CancellationToken cancel)
		{
			var chatId = data.Message?.Chat?.Id ?? 0L;
			var userId = data.From?.Id ?? 0L;
			if (chatId == 0) return;

			_ctx.Set(data.Message?.Chat);
			_ctx.Set(data.From);

			if (_workflowStore != null && _engine != null && _workflowRenderer != null)
			{
				var wfContext = new WorkflowContext { ChatId = chatId, UserId = userId };
				var session = await _workflowStore.TryLoadAsync(wfContext, cancel).ConfigureAwait(false);
				if (session != null)
				{
					var request = WorkflowRequestFactory.FromCallback(data, chatId, userId);
					var result = await _engine.HandleAsync(request, cancel).ConfigureAwait(false);
					await _workflowRenderer.RenderAsync(result, cancel).ConfigureAwait(false);
					await SafeAnswerCallbackAsync(data.Id, cancel);
					return;
				}
			}

			if (!string.IsNullOrWhiteSpace(data.Data) && _cliParser != null)
			{
				var parseResult = _cliParser.ParseAsCallback(data.Data);
				if (parseResult is ReadyToInvokeResult ready && _cliFactory != null)
				{
					var wfContext = new WorkflowContext { ChatId = chatId, UserId = userId };
					var command = _cliFactory.CreateCommand(ready.CommandPath, ready.CollectedValues, wfContext);
					if (command is ICommand cmd)
						await _mm.Send(cmd, cancel).ConfigureAwait(false);
					await SafeAnswerCallbackAsync(data.Id, cancel);
					return;
				}
				if (parseResult is NeedParametersResult need && _workflowStore != null && _engine != null && _workflowRenderer != null)
				{
					var session = new WorkflowSession
					{
						ChatId = chatId,
						UserId = userId,
						WorkflowId = CliParameterCollectionWorkflowDescriptor.Id,
						CurrentStep = WorkflowStepId.From<CollectParameterStep>(),
					};
					session.Set(CliParameterCollectionState.CommandPath, need.CommandPath);
					session.Set(CliParameterCollectionState.MissingList, string.Join(",", need.MissingParameters.Select(p => p.SymbolName)));
					session.Set(CliParameterCollectionState.CurrentIndex, "0");
					foreach (var (k, v) in need.Collected)
						session.Set(CliParameterCollectionState.ValuePrefix + k, v);
					await _workflowStore.CreateAsync(session, cancel).ConfigureAwait(false);
					var request = WorkflowRequestFactory.Start(chatId, userId);
					var result = await _engine.HandleAsync(request, cancel).ConfigureAwait(false);
					await _workflowRenderer.RenderAsync(result, cancel).ConfigureAwait(false);
					await SafeAnswerCallbackAsync(data.Id, cancel);
					return;
				}
			}

			// Fallback: pass to workflow (e.g. for inline button steps)
			if (_engine != null)
				await _engine.HandleAsync(WorkflowRequestFactory.FromCallback(data, chatId, userId), cancel).ConfigureAwait(false);
			await SafeAnswerCallbackAsync(data.Id, cancel);
		}

		private async Task SafeAnswerCallbackAsync(string callbackQueryId, CancellationToken ct, string? text = null)
		{
			try
			{
				await _bot.ApiClient.AnswerCallbackQuery(callbackQueryId, text, cancellationToken: ct);
			}
			catch
			{
				// ignore
			}
		}
	}
} 
