using System.Text;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using MissAlise.Application;
using MissAlise.Application.Interfaces;
using MissAlise.Application.UseCases.Sync;
using MissAlise.Background;
using MissAlise.Bot;
using MissAlise.DataBase;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive;
using MissAlise.Utils;
using MissAlise.Worker.Background;
using MissAlise.Worker.Background.Handlers;

namespace MissAlise.Worker
{
	public class Program
	{
		public static void Main(string[] args)
		{

			var builder = WebApplication.CreateBuilder(args);
			builder.AddServiceDefaults();

			builder.Services.AddApplication().AddPersistance(builder.Configuration, true);

			builder.Services.AddHttpLogging(opts => opts.LoggingFields = HttpLoggingFields.RequestProperties);
			builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Information);

			builder.Services.AddBackgroundServer<MissAliseBackgroundServer>();
			builder.Services
				.AddBackgroundJob<UpdateUsersJob, UpdateUsersJobHandler>(
					builder => builder.SetDescription("Обновление пользователей").AddTrigger(new UpdateUsersJob(64), "Ежеминутно").SetDelay(Time.Minute)
				).AddBackgroundJob<SyncDataJob, SyncBackgroundTaskHandler>(
					builder => builder.SetDescription("Синхронизация данных").AddTrigger(new SyncDataJob(64), "Полуминутно").SetDelay(Time.Minute / 2)
				).AddBackgroundJob<SyncOneDriveFolderJob, SyncOneDriveFolderJobHandler>(
					builder => builder.SetDescription("Синхронизация папки OneDrive")//.AddTrigger(new SyncOneDriveFolderJob(default,default), "1 min").SetDelay(Time.Minute)
				);

			builder.Services
				.AddOneDriveService(builder.Configuration.GetSection(nameof(AzureAd)))
				.AddOneDriveHandling()
				.AddBotService(builder.Configuration.GetSection("BotConfiguration"));
				//.AddScoped<IAsyncHandler<SyncCommand>, SyncCommandHandler>();

			var host = builder.Build();

			host.MapGet("/signin-oidc", Signin);

			host.Run();
		}

		static async Task<IResult> Signin([FromServices] AzureAd config, [FromServices] HttpClient client, [FromQuery] string code, [FromQuery] string state, HttpContext ctx, CancellationToken cancel)
		{
			if (ctx.Request.Headers.Referer.Any(refer => refer == config.Instance || refer == "https://login.live.com/" || refer == "https://account.live.com/") == false)
				return Results.Forbid();

			var response = await ctx.RequestServices.GetRequiredService<IOneDriveCredentialsService>().GetCredentialsByCode(config, code, cancel);

			if (!response.IsSuccessful)
				return Results.Problem(response.Error.ToString(), null, (int)response.StatusCode);

			_ = ctx.RequestServices.GetService<IAuthorizationCompleter>()?.AuthorizationCompleted(state, response.Content, cancel);
			return Results.Text("<html><body>Авторизация закончена успешно. Вы можете закрыть это окно</body></html>", "text/html", Encoding.UTF8, 200);
		}
	}
}