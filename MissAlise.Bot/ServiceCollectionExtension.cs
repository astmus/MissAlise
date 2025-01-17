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
				.AddSingleton<Bot>().Configure<BotConfiguration>(botConfig.GetSection(nameof(BotConfiguration)))				
				.AddTransient<IAuthorizationCompleter, AuthorizationCompleteHandler>()
				.AddHostedService<Bot.UpdateReceiver>()
				.AddHostedService<Bot.UpdateHandler>()
				.AddHttpClient("bot")
				.UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2)) // Recreate connection every 2 minutes
				.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

			services.AddScoped<IAsyncHandler<Message>, ChatMessageHandler>();
			return services;
		}

		static internal Chat GetCurrentChat(this Update update)
			=> update.GetCurrentMessage()?.Chat ?? new Chat() { Id = update.GetCurrentMessage()?.From?.Id ?? update.InlineQuery?.From.Id ?? update.CallbackQuery?.From.Id ?? update.ChosenInlineResult.From.Id };

		static internal Message GetCurrentMessage(this Update update)
			=> update.Message ?? update.CallbackQuery?.Message ?? update.EditedMessage ?? update.ChannelPost ?? update.EditedChannelPost ?? update.Message?.PinnedMessage ?? default;
	}
}
