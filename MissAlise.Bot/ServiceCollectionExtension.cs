using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;
using MissAlise.Bot.Handlers;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddBotService(this IServiceCollection services, IConfigurationSection botConfig)
		{
			services
				.AddSingleton<Bot<UpdateExt>, MissAliseBot>()
				.AddHostedService<MissAliseBot.UpdateReceiver>()
				.AddHostedService<MissAliseBot.UpdateHandler>()
				.Configure<BotConfiguration>(botConfig);

			services.AddScoped<IAsyncHandler<Message>, ChatMessageHandler>();
			return services;
		}

		static internal User GetCurrentUser(this Update update)
		{
			var msg = update.GetCurrentMessage();
			return msg?.From ?? update.InlineQuery?.From ?? update.CallbackQuery?.From ?? update.ChosenInlineResult.From;
		}

		static internal bool IsBotCommand(this Update update)
			=> update.Message.Entities?.Any(e => e.Type == Telegram.Bot.Types.Enums.MessageEntityType.BotCommand) == true;

		static internal Chat GetCurrentChat(this Update update)
			=> update.GetCurrentMessage()?.Chat ?? new Chat() { Id = update.GetCurrentMessage()?.From?.Id ?? update.InlineQuery?.From.Id ?? update.CallbackQuery?.From.Id ?? update.ChosenInlineResult.From.Id };

		static internal Message GetCurrentMessage(this Update update)
			=> update.Message ?? update.CallbackQuery?.Message ?? update.EditedMessage ?? update.ChannelPost ?? update.EditedChannelPost ?? update.Message?.PinnedMessage;
	}
}
