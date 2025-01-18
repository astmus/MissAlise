//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using MissAlise.Application;
//using MissAlise.Application.Interfaces;
//using MissAlise.Bot.Handlers;
//using Telegram.Bot.Types;

//namespace MissAlise.Bot;

//public class Program
//{
//	public static void Main(string[] args)
//	{
//		var builder = Host.CreateApplicationBuilder(args);
//		builder.AddServiceDefaults();

//		builder.Services.AddApplication(builder.Configuration)
//				.AddScoped<IAsyncHandler<Message>, ChatMessageHandler>()
//				.AddSingleton<BotWorker>().Configure<BotConfiguration>(builder.Configuration.GetSection(nameof(BotConfiguration)))
//				.AddTransient<IAuthorizationCompleter, AuthorizationCompleteHandler>()
//				.AddHostedService<BotWorker.UpdateReceiver>()
//				.AddHostedService<BotWorker.UpdateHandler>()
//				.AddHttpClient("bot")
//				.UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2)) // Recreate connection every 2 minutes
//				.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

//		var host = builder.Build();
//		host.Run();
//	}
//}