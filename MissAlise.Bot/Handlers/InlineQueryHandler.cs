using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.Workflow;
using MissAlise.Workflow;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace MissAlise.TelegramBot.Handlers
{
	internal class InlineQueryHandler : BotAsyncHandlerBase<InlineQuery>
	{
		private readonly ILogger<InlineQueryHandler> _log;
		private readonly IWorkflowStateStore? _workflowStore;
		private readonly IWorkflowCoordinator? _engine;
		private readonly IWorkflowResultRenderer? _workflowRenderer;

		public InlineQueryHandler(
			IHandleContext ctx,
			ILogger<InlineQueryHandler> log,
			Bot bot,			
			IWorkflowStateStore? workflowStore = null,
			IWorkflowCoordinator? engine = null,
			IWorkflowResultRenderer? workflowRenderer = null) : base(bot, ctx)
		{
			_log = log;
			_workflowStore = workflowStore;
			_engine = engine;
			_workflowRenderer = workflowRenderer;
		}

		protected override async Task HandleAsync(InlineQuery data, CancellationToken cancel)
		{
			var q = data.Query ?? string.Empty;
			var leafs = _bot.Definition.SearchLeafCommands(q).Take(20).ToArray();

			InlineQueryResult[] results = new InlineQueryResultArticle[leafs.Length];

			for (var i = 0; i < leafs.Length; i++)
			{
				var leaf = leafs[i];
				var title = "/" + leaf.Name;
				var desc = leaf.Description ?? "";

				results[i] = new InlineQueryResultArticle(
					id: $"cmd:{i}:{leaf.Name}",
					title: title,
					inputMessageContent: new InputTextMessageContent(title))
				{
					Description = desc
				};
			}

			await _bot.ApiClient.AnswerInlineQuery(
				inlineQueryId: data.Id,
				results: results,
				isPersonal: true,
				cacheTime: 0,
				cancellationToken: cancel);
			// Inline search: could answer with InlineQueryResultArticle list for suggestions (second-level commands, etc.)
		}
	}
} 
