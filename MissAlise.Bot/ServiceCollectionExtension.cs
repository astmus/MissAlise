using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Background;
using MissAlise.DataBase.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MongoDB.Driver;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddBotService(this IServiceCollection services, IConfiguration appConfig)
		{
			services
				.AddScoped<IHandleContext, HandleContext>()
				.AddScoped<IContextItems, ContextItems>()
				.AddSingleton<Bot>().Configure<BotConfig>(appConfig.GetSection(nameof(BotConfig)))				
				.AddTransient<IAuthorizationCompleter, AuthorizationCompleteHandler>()
				.AddHostedService<Bot.UpdateReceiver>()
				.AddHostedService<Bot.UpdateHandler>()
				.AddHttpClient("bot")
				.UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2)) // Recreate connection every 2 minutes
				.SetHandlerLifetime(Timeout.InfiniteTimeSpan);
			
			return services;
		}

		public static IServiceCollection AddChatMessageHandler<THandler>(this IServiceCollection services) where THandler:class, IAsyncHandler<Message>
			=> services.AddScoped<IAsyncHandler<Message>, THandler>();		

		static internal Chat GetCurrentChat(this Update update)
			=> update.GetCurrentMessage()?.Chat ?? new Chat() { Id = update.GetCurrentMessage()?.From?.Id ?? update.InlineQuery?.From.Id ?? update.CallbackQuery?.From.Id ?? update.ChosenInlineResult.From.Id };

		static internal Message GetCurrentMessage(this Update update)
			=> update.Message ?? update.CallbackQuery?.Message ?? update.EditedMessage ?? update.ChannelPost ?? update.EditedChannelPost ?? update.Message?.PinnedMessage ?? default;
	}
}
