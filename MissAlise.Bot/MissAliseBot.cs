using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissAlise.Application.Commands;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot
{
	internal class MissAliseBot : Bot<UpdateExt>
	{			
		private readonly ILogger<MissAliseBot> _log;		

		public MissAliseBot(IOptions<BotConfiguration> options, ILogger<MissAliseBot> log) : base(options,log)
		{
			botOptions = options.Value;
			_log = log;

			//ApiClient = new TelegramBotClient(botOptions.ApiKey, HttpConnection);
			//receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}

		protected override IEnumerable<RelayCommand> BotCommands =>
		[
			new ("start", null, cmd =>
				{
					return new StartCommand();
				}),
			new RelayCommand("sync", "синхронизировать файлы", cmd =>
				{
					return new SyncCommand(default);
				}),
			new RelayCommand("test", "тестовая комманда", cmd =>
				{
					return new TestCommand();
				})
		];

		//public override Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		//{
		//	return base.HandleErrorAsync(exception, cancellationToken);
		//}
	}
}
