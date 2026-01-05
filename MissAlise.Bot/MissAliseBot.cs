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
	internal class MissAliseBot : Bot<UpdateExt>
	{			
		private readonly ILogger<MissAliseBot> log;		

		public MissAliseBot(IOptions<BotConfiguration> options, ILogger<MissAliseBot> log) : base(options,log)
		{
			botOptions = options.Value;
			this.log = log;

			ApiClient = new TelegramBotClient(botOptions.ApiKey, HttpConnection);
			receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
		}

		protected override IEnumerable<RelayCommand> BotCommands =>
		[
			new ("start", null, cmd =>
				{
					return new StartCommand();
				}),

			new RelayCommand("sync", "синхронизировать файлы", cmd =>
				{
					return new SyncCommand();
				})
		];

		//public override Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		//{
		//	return base.HandleErrorAsync(exception, cancellationToken);
		//}
	}
}
