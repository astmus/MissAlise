using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal partial class BotWorker
	{
		public HttpClient HttpConnection { get; }
		public ITelegramBotClient Client { get; }

		private User botInfo;
		private BotConfiguration botOptions;
		private readonly ILogger<BotWorker> log;
		private ReceiverOptions receiveOptions;
		private readonly Channel<Update> pendingUpdates = Channel.CreateUnbounded<Update>(
			new()
			{
				SingleReader = true,
				SingleWriter = true,
			}
		);

		public IEnumerable<BotCommand> Commands => [new BotCommand() { Command = "sync", Description = "синхронизировать файлы" }];
		public BotWorker(IOptions<BotConfiguration> options, ILogger<BotWorker> log)
		{
			botOptions = options.Value;
			this.log = log;
			//HttpConnection = factory.CreateClient("bot");
			Client = new TelegramBotClient(botOptions.ApiKey, HttpConnection);
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}

		public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			log.LogError(exception, exception.Message, default);
			return Task.CompletedTask;
		}
	}
}
