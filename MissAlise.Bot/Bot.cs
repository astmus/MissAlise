using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Services.Sync;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal class ParentCommand : Command
	{
		public ParentCommand(string name, string? description = null) : base(name, description)
		{
		}

		public Func<ICommand> CreateSubcommand { get; set; }
	}

	internal interface ICommandParent
	{
		public ICommand CreateSubcommand();
	}

	internal partial class Bot
	{
		public HttpClient HttpConnection { get; }
		public ITelegramBotClient Client { get; init; }
	}

	internal partial class Bot<TUpdate> : Bot where TUpdate : Update
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

		public IEnumerable<BotCommand> Commands { get; } =
		[
			new BotCommand<SyncCommand>() { Command = "sync", Description = "синхронизировать файлы" }
		];

		public Bot(IOptions<BotConfiguration> options, ILogger<Bot<TUpdate>> log)
		{
			botOptions = options.Value;
			this.log = log;

			Client = new TelegramBotClient(botOptions.ApiKey, HttpConnection);
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}

		public virtual Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			log.LogError(exception, exception.Message, default);
			return Task.CompletedTask;
		}

		RootCommand root;
		public ICommand Create(string rawCommand)
		{
			ICommand resultCommand = null;
			if (root == null)
			{
				root = new RootCommand();
				var commands = Commands.Select(bc =>
					new ParentCommand(bc.Command, description: bc.Description) { CreateSubcommand = (bc as ICommandParent).CreateSubcommand });

				foreach (var cmd in commands)
				{
					cmd.AddAlias("/" + cmd.Name);
					root.AddCommand(cmd);
					//cmd.AddArgument(new Argument<DateTime>("f"));
				}
			}

			var result = root.Parse(rawCommand);
			if (result.Errors.Any())
				_ = HandleErrorAsync(new AggregateException(result.Errors.Select(error => new Exception(error.Message))), default);
			else
				resultCommand = (result.CommandResult.Command as ParentCommand).CreateSubcommand();

			return resultCommand;
		}

		public virtual UpdatesSource<TUpdate> UpdatesSource
			=> updatesSource ?? (updatesSource = new UpdatesSource<TUpdate>(Client, receiveOptions, HandleErrorAsync));
	}
}
