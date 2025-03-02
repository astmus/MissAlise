using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;

using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal partial class BotWorker
	{
		public class UpdateHandler : BackgroundService
		{
			private readonly ILogger<UpdateHandler> logger;
			private readonly IServiceScopeFactory factory;
			private readonly BotWorker bot;			
			public UpdateHandler(ILogger<UpdateHandler> logger, IServiceScopeFactory factory, BotWorker bot)
			{
				this.logger = logger;
				this.factory = factory;
				this.bot = bot;
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				while (!cancel.IsCancellationRequested)
				{
					try
					{
						await foreach (var update in bot.pendingUpdates.Reader.ReadAllAsync(cancel))
						{
							logger.LogInformation("Got update {Id}", update.Id);
							_ = HandleUpdateAsync(bot.Client, update, cancel);
						}
					}
					catch (Exception error)
					{
						logger.LogError(error, error.Message);
					}					
				}
			}			

			public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancel)
			{
				try
				{					
					using var handleScope = factory.CreateScope();
					var services = handleScope.ServiceProvider;
					//var manager = services.GetRequiredService<UserManager<AppUser>>();
					//var user = await manager.FindByIdAsync(update.GetCurrentMessage().From.Id.ToString());

					//if (user == null)
					//{
					//	var profiles = services.GetRequiredService<IUserProfilesRepository>();
					//	var tmpUser = update.GetCurrentMessage().From;
					//	await profiles.AddPendingUser(new Entities.OneDrive.User()
					//	{
					//		Id = tmpUser.Id.ToString(),
					//		DisplayName = tmpUser.Username,
					//		GivenName = tmpUser.FirstName,
					//		Surname = tmpUser.LastName ?? string.Empty
					//	}, cancel);

					//	var link = services.GetRequiredService<AzureAd>().AuthorizeLink(update.Message.Chat.Id.ToString());
					//	await bot.Client.DeleteMyCommands().ConfigureAwait(false);
					//	await bot.Client.SetChatMenuButton(update.Message.Chat.Id, new MenuButtonWebApp() { Text = "Авторизоваться", WebApp = new WebAppInfo(link.ToString()) }, cancel).ConfigureAwait(false); 
					//	await bot.Client.SendMessage(update.Message.Chat, "Необходима авторизация", parseMode: ParseMode.MarkdownV2, cancellationToken: cancel).ConfigureAwait(false);
					//	return;
					//}

					var sender = update.GetCurrentUser();
					var ctx = services.GetRequiredService<IHandleContext>();					
					ctx.Set(update);
					ctx.Set(new Claimant(sender.Id.ToString(), sender.Username));
					ctx.Set(sender);

					Task currentTask = update switch
					{
						//{ Command: not null } => HandleUpdateAsync(update, update.Command, stoppingToken),
						{ CallbackQuery: not null } => InvokeHandlerAsync(services, update.CallbackQuery, cancel),
						{ InlineQuery: not null } => InvokeHandlerAsync(services, update.InlineQuery, cancel),						
						//{ ChosenInlineResult: not null } => Task.CompletedTask,
						_ => null
						//{ EditedMessage: not null } => UpdateType.EditedMessage,
						//{ ChannelPost: not null } => UpdateType.ChannelPost,
						//{ EditedChannelPost: not null } => UpdateType.EditedChannelPost,
						//{ MessageReaction: not null } => UpdateType.MessageReaction,
						//{ MessageReactionCount: not null } => UpdateType.MessageReactionCount,
						//{ ShippingQuery: not null } => UpdateType.ShippingQuery,
						//{ PreCheckoutQuery: not null } => UpdateType.PreCheckoutQuery,
						//{ Poll: not null } => UpdateType.Poll,
						//{ PollAnswer: not null } => UpdateType.PollAnswer,
						//{ MyChatMember: not null } => UpdateType.MyChatMember,
						//{ ChatMember: not null } => UpdateType.ChatMember,
						//{ ChatJoinRequest: not null } => UpdateType.ChatJoinRequest,
						//{ ChatBoost: not null } => UpdateType.ChatBoost,
						//{ RemovedChatBoost: not null } => UpdateType.RemovedChatBoost
					};
				
					currentTask ??= InvokeHandlerAsync(services, update.GetCurrentMessage(), cancel);
					await currentTask.ConfigureAwait(false);
				}
				catch (Exception error)
				{
					logger.LogError(error, error.Message);
				}
			}

			private Task InvokeHandlerAsync<TUpdateItem>(IServiceProvider sp, TUpdateItem updateItem, CancellationToken cancel) where TUpdateItem : class
			{
				var handler = sp.GetRequiredService<IAsyncHandler<TUpdateItem>>();
				return handler.InvokeAsync(updateItem , cancel);
			}

			private Task HandleSearchAsync(Update update, InlineQuery inlineQuery, object stoppingToken) => throw new NotImplementedException();
			private Task HandleCallbackAsync(Update update, CallbackQuery callbackQuery, object stoppingToken) => throw new NotImplementedException();
		}
	}
}

