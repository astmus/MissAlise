using MissAlise.Application.Interfaces;
using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Workflow;

internal sealed class TelegramWorkflowResultVisitor : WorkflowResultVisitor
{
	private readonly Bot _bot;

	public TelegramWorkflowResultVisitor(IHandleContext context, Bot bot)
		: base(context)
	{
		_bot = bot;
	}

	protected override async Task VisitMessageAsync(Message message, WorkflowHandleResult result, CancellationToken cancellationToken)
	{
		var chatId = message.Chat?.Id ?? 0L;
		if (chatId == 0)
		{
			return;
		}

		Message response = null;
		//if (result.EditMessageId is int messageId)
		//	response = await WorkflowPresentationSender.EditAsync(_bot.ApiClient, chatId, messageId, result.Presentation, cancellationToken);
		//else
		response = await WorkflowPresentationSender.SendAsync(_bot.ApiClient, message.Chat, result.Presentation, cancellationToken);
	}

	protected override Task VisitCallbackQueryAsync(CallbackQuery callbackQuery, WorkflowHandleResult result, CancellationToken cancellationToken)
	{
		if (callbackQuery.Message != null)
		{
			return WorkflowPresentationSender.EditAsync(_bot.ApiClient, callbackQuery.Message, result.Presentation, cancellationToken);
		}

		var chatId = callbackQuery.Message?.Chat?.Id ?? callbackQuery.From?.Id ?? 0L;
		if (chatId == 0)
		{
			return Task.CompletedTask;
		}

		return WorkflowPresentationSender.EditAsync(_bot.ApiClient, callbackQuery.Message, result.Presentation, cancellationToken);
	}

	protected override Task VisitInlineQueryAsync(InlineQuery inlineQuery, WorkflowHandleResult result, CancellationToken cancellationToken)
	{
		var chatId = inlineQuery.From?.Id ?? 0L;
		if (chatId == 0)
		{
			return Task.CompletedTask;
		}

		return WorkflowPresentationSender.SendAsync(_bot.ApiClient, chatId, result.Presentation, cancellationToken);
	}
}
