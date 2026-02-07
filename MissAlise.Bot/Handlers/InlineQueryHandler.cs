using MediatR;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.CommandLine;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers
{
	internal class InlineQueryHandler : BotAsyncHandlerBase<InlineQuery>
	{
		private readonly IHandleContext _ctx;
		private readonly ILogger<InlineQueryHandler> _log;
		private readonly IMediator _mm;
		private readonly Bot _bot;
		private readonly IBotCommandLineParser? _cliParser;
		private readonly IBotCommandFactory? _cliFactory;
		private readonly IWorkflowStateStore? _workflowStore;
		private readonly IWorkflowCoordinator? _engine;
		private readonly IWorkflowResultRenderer? _workflowRenderer;

		public InlineQueryHandler(
			IHandleContext ctx,
			ILogger<InlineQueryHandler> log,
			IMediator mm,
			Bot bot,
			IBotCommandLineParser? cliParser = null,
			IBotCommandFactory? cliFactory = null,
			IWorkflowStateStore? workflowStore = null,
			IWorkflowCoordinator? engine = null,
			IWorkflowResultRenderer? workflowRenderer = null) : base(bot)
		{
			_log = log;
			_mm = mm;
			_bot = bot;
			_ctx = ctx;
			_cliParser = cliParser;
			_cliFactory = cliFactory;
			_workflowStore = workflowStore;
			_engine = engine;
			_workflowRenderer = workflowRenderer;
		}

		protected override async Task HandleAsync(InlineQuery data, CancellationToken cancel)
		{
			var userId = data.From?.Id ?? 0L;
			if (_workflowStore != null && _engine != null && _workflowRenderer != null && userId != 0)
			{
				var wfContext = new WorkflowContext { ChatId = userId, UserId = userId };
				var session = await _workflowStore.TryLoadAsync(wfContext, cancel).ConfigureAwait(false);
				if (session != null)
				{
					var request = WorkflowRequestFactory.FromInlineQuery(data);
					var result = await _engine.HandleAsync(request, cancel).ConfigureAwait(false);
					await _workflowRenderer.RenderAsync(result, cancel).ConfigureAwait(false);
					return;
				}
			}

			if (string.IsNullOrWhiteSpace(data.Query) || _cliParser == null || _cliFactory == null)
				return;

			var parseResult = _cliParser.ParseAsInlineQuery(data.Query);
			if (parseResult is ReadyToInvokeResult ready)
			{
				var wfContext = new WorkflowContext { ChatId = data.From.Id, UserId = data.From.Id };
				var command = _cliFactory.CreateCommand(ready.CommandPath, ready.CollectedValues, wfContext);
				if (command is ICommand cmd)
					await _mm.Send(cmd, cancel).ConfigureAwait(false);
			}
			// Inline search: could answer with InlineQueryResultArticle list for suggestions (second-level commands, etc.)
		}
	}
} 
