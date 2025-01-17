using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal partial class Bot
	{
		public HttpClient HttpConnection { get; }		
		public ITelegramBotClient Client { get; }

		private User botInfo;
		private IOptions<BotConfiguration> botOptions;		
		private readonly ILogger<Bot> log;
		private ReceiverOptions receiveOptions;
		private readonly Channel<Update> pendingUpdates = Channel.CreateUnbounded<Update>(
			new()
			{
				SingleReader = true,
				SingleWriter = true,
			}
		);

		public IEnumerable<BotCommand> Commands => [new BotCommand() { Command = "sync", Description = "синхронизировать файлы" }];
		public Bot(IOptions<BotConfiguration> options, IHttpClientFactory factory, ILogger<Bot> log)
		{
			botOptions = options;			
			this.log = log;
			HttpConnection = factory.CreateClient("bot");
			Client = new TelegramBotClient(botOptions.Value.ApiKey, HttpConnection);
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };			
		}

		public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			log.LogError(exception,default,default);
			return Task.CompletedTask;
		}
	}
}
