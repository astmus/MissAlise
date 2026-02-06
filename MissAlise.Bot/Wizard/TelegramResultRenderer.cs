using MissAlise.TelegramBot.Results;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MissAlise.TelegramBot.Wizard;

public interface ITelegramResultRenderer
{
	Task RenderAsync(long chatId, PresentationResult result, CancellationToken ct);
}

internal sealed class TelegramResultRenderer : ITelegramResultRenderer
{
	private readonly Bot _bot;

	public TelegramResultRenderer(Bot bot) => _bot = bot;

	public async Task RenderAsync(long chatId, PresentationResult result, CancellationToken ct)
	{
		switch (result)
		{
			case MessageResult mr:
				await Send(chatId, mr.Message, ct);
				break;

			case MultiMessageResult mm:
				foreach (var m in mm.Messages)
					await Send(chatId, m, ct);
				break;

			case ErrorResult er:
				await Send(chatId, new OutMessage(
					$"? {er.Title}\n{er.Details}",
					OutParseMode.Markdown
				), ct);
				break;

			default:
				// Empty or unknown
				break;
		}
	}

	private Task Send(long chatId, OutMessage msg, CancellationToken ct)
	{
		var pm = msg.ParseMode switch
		{
			OutParseMode.Markdown => ParseMode.Markdown,
			OutParseMode.Html => ParseMode.Html,
			_ => ParseMode.None
		};

		return _bot.ApiClient.SendTextMessageAsync(
			chatId: chatId,
			text: msg.Text,
			parseMode: pm,
			replyMarkup: msg.Keyboard is null ? null : MapKeyboard(msg.Keyboard),
			cancellationToken: ct);
	}

	private static InlineKeyboardMarkup MapKeyboard(OutKeyboard k)
	{
		var rows = k.Rows
			.Select(r => r.Select(MapButton).ToArray())
			.ToArray();

		return new InlineKeyboardMarkup(rows);
	}

	private static InlineKeyboardButton MapButton(OutButton b)
	{
		if (!string.IsNullOrWhiteSpace(b.Url))
			return InlineKeyboardButton.WithUrl(b.Text, b.Url);

		return InlineKeyboardButton.WithCallbackData(b.Text, b.CallbackData ?? string.Empty);
	}
}