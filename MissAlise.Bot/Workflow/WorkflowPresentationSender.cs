using MissAlise.Workflow.Presentation;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MissAlise.TelegramBot.Workflow;

internal static class WorkflowPresentationSender
{
    public static Task SendAsync(ITelegramBotClient client, long chatId, WorkflowPresentation presentation, CancellationToken cancellationToken)
    {
        ReplyMarkup? markup = null;

        if (presentation.Buttons.Count > 0)
        {
            var rows = presentation.Buttons
                .Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.Payload) })
                .ToArray();

            markup = new InlineKeyboardMarkup(rows);
        }

        return client.SendTextMessageAsync(
            chatId: chatId,
            text: presentation.Text,
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }

    public static Task EditAsync(ITelegramBotClient client, Message message, WorkflowPresentation presentation, CancellationToken cancellationToken)
    {
        ReplyMarkup? markup = null;

        if (presentation.Buttons.Count > 0)
        {
            var rows = presentation.Buttons
                .Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.Payload) })
                .ToArray();

            markup = new InlineKeyboardMarkup(rows);
        }

        return client.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: presentation.Text,
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }
}
