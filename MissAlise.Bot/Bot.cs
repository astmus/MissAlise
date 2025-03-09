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
		public ITelegramBotClient Client { get; init; }
	}

	internal partial class Bot<TUpdate> : Bot where TUpdate: Update
	{
		private User Information;
		private BotConfiguration botOptions;
		private readonly ILogger<Bot<TUpdate>> log;
		private ReceiverOptions receiveOptions;
		private UpdatesSource<TUpdate> updatesSource;
		private readonly Channel<TUpdate> Updates = Channel.CreateUnbounded<TUpdate>(
			new()
			{
				SingleReader = true,
				SingleWriter = true,
			}
		);

		public IEnumerable<BotCommand> Commands => [new BotCommand() { Command = "sync", Description = "синхронизировать файлы" }];
		public Bot(IOptions<BotConfiguration> options, ILogger<Bot<TUpdate>> log)
		{
			botOptions = options.Value;
			this.log = log;
			//HttpConnection = factory.CreateClient("bot");
			Client = new TelegramBotClient(botOptions.ApiKey, HttpConnection);
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}

		public virtual Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			log.LogError(exception, exception.Message, default);
			return Task.CompletedTask;
		}

		public virtual UpdatesSource<TUpdate> UpdatesSource
			=> updatesSource ?? (updatesSource = new UpdatesSource<TUpdate>(Client, receiveOptions, HandleErrorAsync));
	}
}
