using System.Collections.Specialized;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal partial class Bot
	{
		internal class UpdateReceiver : BackgroundService
		{
			private readonly ILogger<UpdateReceiver> logger;
			private readonly IServiceScopeFactory factory;
			private Bot bot;
			private IServiceScope scope;
			public UpdateReceiver(ILogger<UpdateReceiver> logger, IServiceScopeFactory factory)
			{
				this.logger = logger;
				this.factory = factory;
			}
			public override async Task StartAsync(CancellationToken cancellationToken)
			{
				scope = factory.CreateScope();
				bot = scope.ServiceProvider.GetRequiredService<Bot>();				
				
				try
				{
					bot.botInfo = await bot.Client.GetMe(cancellationToken).ConfigureAwait(false);
					await bot.Client.DeleteMyCommands(cancellationToken:cancellationToken);				
				}
				catch (Exception error)
				{
					logger.LogError(error, "Connect to bot failed");
					await StopAsync(cancellationToken);
					return;
				}
				
				//var app = ConfidentialClientApplicationBuilder.Create(azure.ClientId)
				//	.WithClientSecret(azure.ClientSecret)
				//	.WithAuthority(new Uri(azure.AuthorizeLink))
				//	.WithRedirectUri(azure.RedirectUri)
				//	.Build();
					
				//var url = await app.GetAuthorizationRequestUrl(azure.GetScopes()).ExecuteAsync();
				//var button = new MenuButtonWebApp()
				//{
				//	Text = "Auth",
				//	WebApp = new WebAppInfo(url.ToString())
				//};
				//await bot.botClient.SetChatMenuButton(506545376, button);
				await base.StartAsync(cancellationToken).ConfigureAwait(false); ;
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				try
				{					
					logger.LogInformation("Bot {bot}", bot.botInfo);
					var updatesQueue = new QueuedUpdateReceiver(bot.Client, bot.receiveOptions, bot.HandleErrorAsync);
					
					await foreach (var update in updatesQueue.WithCancellation(cancel))
					{						
						logger.LogInformation("Got update {Id}", update.Id);
						await bot.pendingUpdates.Writer.WriteAsync(update).ConfigureAwait(false);
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
				return	base.StopAsync(cancellationToken);
			}
		}
	}
}
