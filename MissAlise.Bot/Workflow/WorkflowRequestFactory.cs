using MissAlise.Workflow;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Workflow;

internal static class WorkflowRequestFactory
{
    public static WorkflowRequest FromMessage(Message msg, long userId, string? correlationId = null)
    {
        return new WorkflowRequest
        {
            Context = new WorkflowContext
            {
                ChatId = msg.Chat.Id,
                UserId = userId,
                CorrelationId = correlationId,
            },
            Input = new WorkflowInput
            {
                Kind = WorkflowInputKind.Text,
                Text = msg.Text
            }
        };
    }

    public static WorkflowRequest FromCallback(CallbackQuery cq, long chatId, long userId, string? correlationId = null)
    {
        return new WorkflowRequest
        {
            Context = new WorkflowContext
            {
                ChatId = chatId,
                UserId = userId,
                CorrelationId = correlationId,
            },
            Input = new WorkflowInput
            {
                Kind = WorkflowInputKind.Callback,
                Payload = cq.Data
            }
        };
    }

    public static WorkflowRequest FromInlineQuery(InlineQuery iq, string? correlationId = null)
    {
        var userId = iq.From.Id;
        return new WorkflowRequest
        {
            Context = new WorkflowContext
            {
                ChatId = userId,
                UserId = userId,
                CorrelationId = correlationId,
            },
            Input = new WorkflowInput
            {
                Kind = WorkflowInputKind.Text,
                Text = iq.Query
            }
        };
    }

    public static WorkflowRequest Start(long chatId, long userId, string? correlationId = null)
    {
        return new WorkflowRequest
        {
            Context = new WorkflowContext
            {
                ChatId = chatId,
                UserId = userId,
                CorrelationId = correlationId,
            },
            Input = WorkflowInput.Empty
        };
    }
}
