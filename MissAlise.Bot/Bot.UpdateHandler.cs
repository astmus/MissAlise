using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
			private BotWorker bot;
			private IServiceScope currentScope;
			private IServiceProvider services;
			public UpdateHandler(ILogger<UpdateHandler> logger, IServiceScopeFactory factory)
			{
				this.logger = logger;
				this.factory = factory;
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				while (!cancel.IsCancellationRequested)
				{
					currentScope = factory.CreateScope();
					try
					{
						services = currentScope.ServiceProvider;
						bot = services.GetRequiredService<BotWorker>();
						//azure = currentScope.ServiceProvider.GetRequiredService<AzureAd>();

						await foreach (var update in bot.pendingUpdates.Reader.ReadAllAsync(cancel))
						{
							logger.LogInformation("Got update {Id}", update.Id);
							await HandleUpdateAsync(bot.Client, update, cancel);
						}
					}
					catch (Exception error)
					{
						logger.LogError(error, error.Message);
					}
					finally
					{
						currentScope.Dispose();
					}
				}
			}

			async Task<UserProfile> Authorize(ITelegramBotClient botClient, Update update, CancellationToken cancel)
			{
				var sender = update.GetCurrentMessage().From;
				var db = services.GetRequiredService<IUserProfilesRepository>();
				var profile = await db.FindAsync(sender.Id.ToString(), cancel);
				if (profile == null)
				{
					profile = new UserProfile()
					{
						Id = sender.Id.ToString(),
						Telegram = new() { Id = sender.Id.ToString() }
					};
					await db.AddOrReplaceAsync(profile, cancel);
				}

				if (profile.AccessData == null)
				{
					var link = services.GetRequiredService<AzureAd>().AuthorizeLink(update.Message.Chat.Id.ToString());
					await bot.Client.DeleteMyCommands().ConfigureAwait(false);
					await bot.Client.SetChatMenuButton(update.Message.Chat.Id, new MenuButtonWebApp() { Text = "Авторизоваться", WebApp = new WebAppInfo(link.ToString()) }, cancel).ConfigureAwait(false); ;
					await bot.Client.SendMessage(update.Message.Chat, "Необходима авторизация", parseMode: ParseMode.MarkdownV2, cancellationToken: cancel).ConfigureAwait(false); ;
					return null;
				}
				return profile;
			}

			public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancel)
			{
				if (await Authorize(botClient, update, cancel) is not UserProfile profile)
					return;

				//await bot.Client.SetMyCommands(bot.Commands, BotCommandScope.Chat(update.GetCurrentChat().Id), cancellationToken: cancel).ConfigureAwait(false);
				try
				{
					using var handleScope = factory.CreateScope();

					var items = handleScope.ServiceProvider.GetRequiredService<IHandleContext>().Items;
					items.Set(profile);
					items.Set(update);
					var srv = handleScope.ServiceProvider.GetServices<UserProfile>();
					var handler = handleScope.ServiceProvider.GetRequiredService<IAsyncHandler<Message>>();
					Task currentTask = update switch
					{
						//{ Command: not null } => HandleUpdateAsync(update, update.Command, stoppingToken),
						{ CallbackQuery: not null } => HandleCallbackAsync(update, update.CallbackQuery, cancel),
						{ InlineQuery: not null } => HandleSearchAsync(update, update.InlineQuery, cancel),
						{ Message: not null } => handler.InvokeAsync(update.GetCurrentMessage(), cancel),
						//{ ChosenInlineResult: not null } => Task.CompletedTask,
						_ => Task.CompletedTask
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
						//{ RemovedChatBoost: not null } => UpdateType.RemovedChatBoost,
					};
					await currentTask.ConfigureAwait(false);
				}
				catch (Exception error)
				{

					throw;
				}
			}


			private Task HandleSearchAsync(Update update, InlineQuery inlineQuery, object stoppingToken) => throw new NotImplementedException();
			private Task HandleCallbackAsync(Update update, CallbackQuery callbackQuery, object stoppingToken) => throw new NotImplementedException();
		}
	}
}

