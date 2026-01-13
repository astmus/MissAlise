using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissAlise.Application.Commands;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot
{
	internal abstract partial class Bot
	{
		public HttpClient HttpConnection { get; }
		public ITelegramBotClient ApiClient { get; init; }
		public RootCommand BotCommandRoot => _botCommandRoot.Value;

		protected User Information { get; set; }
		protected BotConfiguration botOptions { get; set; }
		protected ReceiverOptions receiveOptions { get; set; }
		protected abstract IEnumerable<RelayCommand> BotCommands { get; }

		protected Lazy<RootCommand> _botCommandRoot;
	}

	internal abstract partial class Bot<TUpdate> : Bot where TUpdate : Update
	{
		private readonly ILogger<Bot<TUpdate>> log;
		private UpdatesSource<TUpdate> updatesSource;
		private readonly Channel<TUpdate> Updates = Channel.CreateUnbounded<TUpdate>(
			new()
			{
				SingleReader = true,
				SingleWriter = true,
			}
		);

		public Bot(IOptions<BotConfiguration> options, ILogger<Bot<TUpdate>> log)
		{
			botOptions = options.Value;
			this.log = log;

			_botCommandRoot = new Lazy<RootCommand>(() =>
			  {
				  var root = new RootCommand();
				  foreach (var cmd in BotCommands)
					  root.AddCommand(cmd);
				  return root;
			  }, LazyThreadSafetyMode.ExecutionAndPublication);

			ApiClient = new TelegramBotClient(botOptions.ApiKey, HttpConnection);
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}

		public virtual Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			log.LogError(exception, exception.Message, default);
			return Task.CompletedTask;
		}

		public ICommand ParseCommand(Message message)
		{
			ICommand resultCommand = null;			

			var result = BotCommandRoot.Parse(message.Text);

			if (result.Errors.Any())
				_ = HandleErrorAsync(new AggregateException(result.Errors.Select(error => new Exception(error.Message))), default);
			else
				resultCommand = (result.CommandResult.Command as RelayCommand).Command;

			return resultCommand;
		}

		public virtual UpdatesSource<TUpdate> UpdatesSource
			=> updatesSource ?? (updatesSource = new UpdatesSource<TUpdate>(ApiClient, receiveOptions, HandleErrorAsync));

	}
}
