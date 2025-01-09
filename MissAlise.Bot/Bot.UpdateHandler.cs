using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal partial class Bot
	{
		public class UpdateHandler : BackgroundService
		{
			private readonly ILogger<UpdateHandler> logger;
			private readonly IServiceScopeFactory factory;
			private Bot bot;

			public UpdateHandler(ILogger<UpdateHandler> logger, IServiceScopeFactory factory)
			{
				this.logger = logger;
				this.factory = factory;
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				try
				{
					using var scope = factory.CreateScope();
					bot = scope.ServiceProvider.GetRequiredService<Bot>();

					await foreach (var update in bot.Updates.Pending.ReadAllAsync(cancel))
					{
						if (cancel.IsCancellationRequested) break;
						logger.LogInformation("Got update {Id}", update.Id);
						await HandleUpdateAsync(bot.botClient, update, cancel).ConfigureAwait(false);
					}
				}
				catch (Exception error)
				{
					logger.LogError(error, error.Message);
				}
			}

			public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
			{
				Task currentTask = Task.CompletedTask;
				var link = "[link](https://login.microsoftonline.com/common/oauth2/v2.0/authorize?scope=offline_access+user.read+files.readwrite+openid+profile+files.readwrite.all&response_type=code&client_id=bfb48d29-c618-4084-a8cc-e28fee1f628d&redirect_uri=https%3A%2F%2F95.139.93.95%2F&prompt=select_account&client_info=1&state=" + update.Message.Chat.Id+")";
				switch (update.Type)
				{
					case UpdateType.Unknown:
					break;
					case UpdateType.Message:
					currentTask = bot.botClient.SendMessage(update.Message.Chat, link,parseMode:ParseMode.MarkdownV2, cancellationToken: cancellationToken);
					break;
					case UpdateType.InlineQuery:
					break;
					case UpdateType.ChosenInlineResult:
					break;
					case UpdateType.CallbackQuery:
					break;
					case UpdateType.EditedMessage:
					break;
					case UpdateType.ChannelPost:
					break;
					case UpdateType.EditedChannelPost:
					break;
					case UpdateType.ShippingQuery:
					break;
					case UpdateType.PreCheckoutQuery:
					break;
					case UpdateType.Poll:
					break;
					case UpdateType.PollAnswer:
					break;
					case UpdateType.MyChatMember:
					break;
					case UpdateType.ChatMember:
					break;
					case UpdateType.ChatJoinRequest:
					break;
					case UpdateType.MessageReaction:
					break;
					case UpdateType.MessageReactionCount:
					break;
					case UpdateType.ChatBoost:
					break;
					case UpdateType.RemovedChatBoost:
					break;
					case UpdateType.BusinessConnection:
					break;
					case UpdateType.BusinessMessage:
					break;
					case UpdateType.EditedBusinessMessage:
					break;
					case UpdateType.DeletedBusinessMessages:
					break;
					case UpdateType.PurchasedPaidMedia:
					break;
					default:
					currentTask = bot.botClient.SendMessage(update.Message.Chat, update.Message.Text, ParseMode.Markdown, cancellationToken: cancellationToken);
					break;
				}
				await currentTask.ConfigureAwait(false);
			}
		}
	}
}
