using MissAlise.Workflow.Presentation;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MissAlise.TelegramBot.Workflow;

internal sealed class TelegramWorkflowRenderer : IWorkflowResultRenderer
{
    private readonly ITelegramBotClient _client;

    public TelegramWorkflowRenderer(Bot bot)
    {
        _client = bot.ApiClient;
    }

    public Task RenderAsync(long chatId, WorkflowPresentation presentation, CancellationToken cancel)
    {
        ReplyMarkup? markup = null;

        if (presentation.Buttons.Count > 0)
        {
            // One button per row (simple & readable). Can be upgraded later.
            var rows = presentation.Buttons
                .Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.Payload) })
                .ToArray();

            markup = new InlineKeyboardMarkup(rows);
        }

        return _client.SendTextMessageAsync(
            chatId: chatId,
            text: presentation.Text,
            replyMarkup: markup,
            cancellationToken: cancel);
    }
}
