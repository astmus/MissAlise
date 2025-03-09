using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal partial class Bot<TUpdate> where TUpdate: Update
	{
		internal class UpdateReceiver : BackgroundService
		{
			private readonly ILogger<UpdateReceiver> logger;
			private Bot<TUpdate> bot;
			private readonly IServiceScope scope;
			public UpdateReceiver(ILogger<UpdateReceiver> logger, IServiceScopeFactory factory)
			{
				this.logger = logger;
				scope = factory.CreateScope();
			}
			public override async Task StartAsync(CancellationToken cancellationToken)
			{
				try
				{
					bot = scope.ServiceProvider.GetRequiredService<Bot<TUpdate>>();
					bot.Information = await bot.Client.GetMe(cancellationToken).ConfigureAwait(false);
					await bot.Client.DeleteMyCommands(cancellationToken: cancellationToken);
				}
				catch (Exception error)
				{
					logger.LogError(error, "Connect to bot failed");
					await StopAsync(cancellationToken);
					scope.Dispose();
					return;
				}

				await base.StartAsync(cancellationToken).ConfigureAwait(false);
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				try
				{
					logger.LogInformation("Bot {bot}", bot.Information);
					
					IAsyncEnumerable<TUpdate> updates = bot.UpdatesSource;
					await foreach (TUpdate update in updates/*.WithCancellation(cancel)*/)
					{
						logger.LogInformation("Got update {Id}", update.Id);
						if (update.IsBotCommand())
						{
							if (update is UpdateExt ext && bot.Create(update.Message.Text) is ICommand command) // временное решение
								ext.Command = command;
							else
								continue;
						}

						await bot.Updates.Writer.WriteAsync(update, cancel).ConfigureAwait(false);
					}

				}
				catch (Exception error)
				{
					logger.LogError(error, error.Message);
				}
			}

			public override Task StopAsync(CancellationToken cancellationToken)
			{
				scope.Dispose();
				return base.StopAsync(cancellationToken);
			}
		}
	}
}
