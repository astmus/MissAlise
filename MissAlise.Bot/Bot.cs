using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot
{
	public abstract partial class Bot
	{
		public Bot(ILogger<Bot> log)
		{
			_log = log;
		}

		public HttpClient HttpConnection { get; }
		public ITelegramBotClient ApiClient { get; init; }
		public BotCommand BotCommandRoot => botCommandRoot.Value;
		public User Info { get; protected set; }

		protected IBotConfiguration botOptions { get; set; }
		protected ReceiverOptions receiveOptions { get; set; }
		protected virtual IEnumerable<BotCommand> BotCommands { get; set; }
		protected Lazy<BotCommand> botCommandRoot;
		protected readonly ILogger<Bot> _log;

		public virtual Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			_log.LogError(exception, exception.Message, default);
			return Task.CompletedTask;
		}

		public ICommand ParseCommand(string message)
		{
			ICommand resultCommand = null;

			//var result = null;//= BotCommandRoot.Parse(message);

			//if (result.Errors.Any())
			//	_ = HandleErrorAsync(new AggregateException(result.Errors.Select(error => new Exception(error.Message))), default);
			//else
				//resultCommand = (result.CommandResult.Command as RelayCommand).Command;

			return resultCommand;
		}
	}

	internal partial class Bot<TUpdate> : Bot where TUpdate : Update
	{
		private UpdatesSource<TUpdate> _updatesSource;
		//private readonly BotDefinition _definition;
		private readonly Channel<TUpdate> Updates = Channel.CreateUnbounded<TUpdate>(
			new()
			{
				SingleReader = true,
				SingleWriter = true,
			}
		);

		public Bot(IOptions<BotConfiguration> options, ILogger<Bot<TUpdate>> log) : base(log)
		{
			botOptions = options.Value;
			//_definition = definition;

			//botCommandRoot = new Lazy<RootCommand>(() =>
			//  {
			//	  var root = new RootCommand();
			//	  foreach (var cmd in BotCommands)
			//		  root.AddCommand(cmd);

			//	  var commands = definition.Commands
			//			  .Where(c => c.IsPublic)
			//			  .Select(c => new RelayCommand(c.Command, c.Description ?? "", rCommand => null));

			//	  foreach (var c in commands)
			//		  root.AddCommand(c);

			//	  return root;
			//  }, LazyThreadSafetyMode.ExecutionAndPublication);

			ApiClient = new TelegramBotClient(botOptions.ApiKey, HttpConnection);

			// ïåðåíåñòè â áèëäåð
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}
		
		public virtual IAsyncEnumerable<TUpdate> UpdatesSource 
			=> _updatesSource ?? (_updatesSource = new UpdatesSource<TUpdate>(ApiClient, receiveOptions, HandleErrorAsync));
	}
}
