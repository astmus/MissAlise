using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot
{
	public static class ServiceCollectionExtension
	{
		static internal User GetCurrentUser(this Update update)
		{
			var msg = update.GetCurrentMessage();
			return msg?.From ?? update.InlineQuery?.From ?? update.CallbackQuery?.From ?? update.ChosenInlineResult.From;
		}

		static internal bool IsBotCommand(this Update update)
			=> update.Message.Entities?.Any(e => e.Type == Telegram.Bot.Types.Enums.MessageEntityType.BotCommand) == true;

		static internal bool IsBotCommand(this Update update, string commandName)
			=> update.IsBotCommand() && update.Message.Text == commandName;

		static internal Chat GetCurrentChat(this Update update)
			=> update.GetCurrentMessage()?.Chat ?? new Chat() { Id = update.GetCurrentMessage()?.From?.Id ?? update.InlineQuery?.From.Id ?? update.CallbackQuery?.From.Id ?? update.ChosenInlineResult.From.Id };

		static internal Message GetCurrentMessage(this Update update)
			=> update.Message ?? update.CallbackQuery?.Message ?? update.EditedMessage ?? update.ChannelPost ?? update.EditedChannelPost ?? update.Message?.PinnedMessage;
	}
}
