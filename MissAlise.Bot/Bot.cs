using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal partial class Bot
	{
		private User botInfo;
		private ITelegramBotClient botClient => initializer.Value;
		private IOptions<BotConfig> botOptions;		
		private readonly ILogger<Bot> log;
		private Lazy<ITelegramBotClient> initializer;
		private UpdatesChannel Updates { get; }

		public Bot(IOptions<BotConfig> options, UpdatesChannel updatesChannel, ILogger<Bot> log)
		{
			botOptions = options;
			Updates = updatesChannel;
			this.log = log;
			initializer = new Lazy<ITelegramBotClient>(() => new TelegramBotClient(botOptions.Value.ApiKey, Updates.Client), true);
		}
		public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			log.LogError(exception,default,default);
			return Task.CompletedTask;
		}
	}
}
