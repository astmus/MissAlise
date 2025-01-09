using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MissAlise.WebApi.Auth;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal partial class Bot
	{
		internal class UpdateReceiver : BackgroundService
		{
			private ITelegramBotClient client;
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
				client = bot.botClient;
				var azure = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetSection(nameof(AzureAd)).Get<AzureAd>();
				bot.botInfo = await client.GetMe(cancellationToken).ConfigureAwait(false);
				var app = ConfidentialClientApplicationBuilder.Create(azure.ClientId)
					.WithClientSecret(azure.ClientSecret)
					.WithAuthority(new Uri(azure.AuthorizeLink))
					.WithRedirectUri(azure.RedirectUri)
					.Build();
					
				var url = await app.GetAuthorizationRequestUrl(azure.GetScopes()).ExecuteAsync();
				var button = new MenuButtonWebApp()
				{
					Text = "Auth",
					WebApp = new WebAppInfo(url.ToString())
				};
				//await bot.botClient.SetChatMenuButton(506545376, button);
				await base.StartAsync(cancellationToken).ConfigureAwait(false); ;
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				try
				{					
					logger.LogInformation("Bot {bot}", bot.botInfo);
					var receiveOptions = new ReceiverOptions() { Limit = 100, AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery, UpdateType.ChosenInlineResult] };
					var receiver = new BlockingUpdateReceiver(client, receiveOptions, bot.HandleErrorAsync);

					await foreach (var update in receiver.WithCancellation(cancel))
					{
						if (cancel.IsCancellationRequested) break;
						logger.LogInformation("Got update {Id}", update.Id);
						await bot.Updates.Incoming.WriteAsync(update).ConfigureAwait(false);
					}
				}
				catch (Exception error)
				{
					logger.LogError(error, error.Message);
				}
			}
		}
	}
}
