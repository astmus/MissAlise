using MissAlise.Workflow.Presentation;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MissAlise.TelegramBot.Workflow;

internal static class WorkflowPresentationSender
{
	public static Task<Message> SendAsync(ITelegramBotClient client, long chatId, WorkflowPresentation presentation, CancellationToken cancellationToken)
	{
		ReplyMarkup? markup = null;

		if (presentation.Buttons.Count > 0)
		{
			var rows = presentation.Buttons
				.Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.Payload) })
				.ToArray();

			markup = new InlineKeyboardMarkup(rows);
		}

		return client.SendMessage(
			chatId: chatId,
			text: presentation.Text,
			replyMarkup: markup,
			cancellationToken: cancellationToken);
	}

	public static Task<Message> SendAsync(ITelegramBotClient client, Chat chat, WorkflowPresentation presentation, CancellationToken cancellationToken)
	{
		return SendAsync(client, chat.Id, presentation, cancellationToken);
	}

	public static Task<Message> EditAsync(ITelegramBotClient client, long chatId, int messageId, WorkflowPresentation presentation, CancellationToken cancellationToken)
	{
		InlineKeyboardMarkup? markup = null;

		if (presentation.Buttons.Count > 0)
		{
			var rows = presentation.Buttons
				.Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.Payload) })
				.ToArray();

			markup = new InlineKeyboardMarkup(rows);
		}

		return client.EditMessageText(
			chatId: chatId,
			messageId: messageId,
			text: presentation.Text,
			replyMarkup: markup,
			cancellationToken: cancellationToken);
	}

	public static Task EditAsync(ITelegramBotClient client, Message message, WorkflowPresentation presentation, CancellationToken cancellationToken)
	{
		return EditAsync(client, message.Chat.Id, message.MessageId, presentation, cancellationToken);
	}

	public static Task DeleteMessageAsync(ITelegramBotClient client, long chatId, int messageId, CancellationToken cancellationToken)
	{
		return client.DeleteMessage(chatId, messageId, cancellationToken);
	}
}
