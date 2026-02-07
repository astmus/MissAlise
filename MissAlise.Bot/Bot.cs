using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.TelegramBot.Building;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot
{
	public abstract partial class Bot
	{
		public Bot(ILogger<Bot> log, BotDefinition definition)
		{
			_log = log;
			this.Definition = definition;
		}

		public ITelegramBotClient ApiClient { get; init; }
		//public BotCommand BotCommandRoot => Definition.;
		public User Info { get; protected set; }

		protected IBotConfiguration botOptions { get; set; }
		protected ReceiverOptions receiveOptions { get; set; }
		protected virtual IEnumerable<BotCommand> BotCommands { get; set; }
		public BotDefinition Definition { get; }

		protected readonly ILogger<Bot> _log;
		protected HttpClient HttpConnection { get; }

		public virtual Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			_log.LogError(exception, exception.Message, default);
			return Task.CompletedTask;
		}
	}

	internal partial class Bot<TUpdate> : Bot where TUpdate : Update
	{
		private UpdatesSource<TUpdate> _updatesSource;
		private readonly Channel<TUpdate> Updates = Channel.CreateUnbounded<TUpdate>(
			new()
			{
				SingleReader = true,
				SingleWriter = true,
			}
		);

		public Bot(IOptions<BotConfiguration> options, ILogger<Bot<TUpdate>> log, BotDefinition definition) : base(log, definition)
		{
			botOptions = options.Value;

			ApiClient = new TelegramBotClient(botOptions.ApiKey, HttpConnection);

			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}
		
		public virtual IAsyncEnumerable<TUpdate> UpdatesSource 
			=> _updatesSource ?? (_updatesSource = new UpdatesSource<TUpdate>(ApiClient, receiveOptions, HandleErrorAsync));
	}
}
