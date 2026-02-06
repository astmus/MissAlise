using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.CommandLine;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers
{
	internal class ChatMessageHandler : BotAsyncHandlerBase<Message>
	{
		private readonly IHandleContext _ctx;
		private readonly ILogger<ChatMessageHandler> _log;
		private readonly IWorkflowCommandSink? _commandSink;
		private readonly IWorkflowCoordinator _engine;
		private readonly IBotCommandLineParser? _cliParser;
		private readonly IBotCommandFactory? _cliFactory;
		private readonly IWorkflowStateStore? _workflowStore;
		private readonly IWorkflowResultRenderer? _workflowRenderer;

		public ChatMessageHandler(
			IHandleContext ctx,
			ILogger<ChatMessageHandler> log,
			Bot bot,
			IWorkflowCoordinator engine,
			IWorkflowCommandSink? commandSink = null,
			IBotCommandLineParser? cliParser = null,
			IBotCommandFactory? cliFactory = null,
			IWorkflowStateStore? workflowStore = null,
			IWorkflowResultRenderer? workflowRenderer = null) : base(bot)
		{
			_log = log;
			_commandSink = commandSink;
			_engine = engine;
			_ctx = ctx;
			_cliParser = cliParser;
			_cliFactory = cliFactory;
			_workflowStore = workflowStore;
			_workflowRenderer = workflowRenderer;
		}

		protected override async Task HandleAsync(Message data, CancellationToken cancel)
		{
			var userId = _ctx.Get<User>()?.Id ?? 0L;
			var chatId = data.Chat?.Id ?? 0L;

			if (chatId != 0 && userId != 0 && _workflowStore != null && _engine != null && _workflowRenderer != null)
			{
				var wfContext = new WorkflowContext { ChatId = chatId, UserId = userId };
				var session = await _workflowStore.TryLoadAsync(wfContext, cancel).ConfigureAwait(false);
				if (session != null)
				{
					var request = WorkflowRequestFactory.FromMessage(data, userId);
					var result = await _engine.HandleAsync(request, cancel).ConfigureAwait(false);
					await _workflowRenderer.RenderAsync(chatId, result.Presentation, cancel).ConfigureAwait(false);
					return;
				}
			}

			if (_ctx.GetCurrent<UpdateExt>().IsBotCommand() && !string.IsNullOrWhiteSpace(data.Text))
			{
				var input = data.Text.Trim();

				if (_cliParser != null)
				{
					var parseResult = _cliParser.ParseAsBotCommand(data.Text);
					if (parseResult is ReadyToInvokeResult ready && _cliFactory != null && _commandSink != null)
					{
						var wfContext = new WorkflowContext { ChatId = chatId, UserId = userId };
						var command = _cliFactory.CreateCommand(ready.CommandPath, ready.CollectedValues, wfContext);
						if (command != null)
							await _commandSink.PublishAsync(command, wfContext, cancel).ConfigureAwait(false);
						return;
					}

					if (parseResult is NeedParametersResult need && _workflowStore != null && _workflowRenderer != null)
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
						await _workflowRenderer.RenderAsync(chatId, result.Presentation, cancel).ConfigureAwait(false);
						return;
					}
				}
			}

			// Fallback: try workflow (e.g. background-sync) if no CLI session was started
			await _engine.HandleAsync(WorkflowRequestFactory.FromMessage(data, userId), cancel).ConfigureAwait(false);
		}
	}
} 
