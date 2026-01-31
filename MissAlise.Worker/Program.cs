using System.Text;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MissAlise.Application.Services.Authentication;
using MissAlise.Interfaces;
using MissAlise.Background;
using MissAlise.OneDrive;
using MissAlise.OneDrive.Auth;
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
				.AddOneDriveService(builder.Configuration)
				.AddOneDriveHandling()
				.AddBotService(builder.Configuration.GetSection("BotConfiguration"));
			//.AddScoped<IAsyncHandler<SyncCommand>, SyncCommandHandler>();
			var host = builder.Build();

			host.MapGet("/signin-oidc", Signin);

			host.Run();
		}

		static async Task<IResult> Signin([FromServices] IOptions<AzureAd> options, [FromQuery] string code, [FromQuery] string state, HttpContext ctx, CancellationToken cancel)
		{
			AzureAd config = options.Value;
			if (ctx.Request.Headers.Referer.Any(refer => refer == config.Instance || refer == "https://login.live.com/" || refer == "https://account.live.com/") == false)
				return Results.Forbid();

			var response = await ctx.RequestServices.GetRequiredService<IOneDriveTokenService>().GetCredentialsByCode(config, code, cancel);
			if (!response.IsSuccessful)
				return Results.Problem(response.Error.ToString(), null, (int)response.StatusCode);

			var ownerResolver = ctx.RequestServices.GetRequiredService<IOwnerResolver>();
			var accessStore = ctx.RequestServices.GetRequiredService<IAccessCredentialsStore>();
			var identity = new MissAlise.ValueObjects.Identity.ExternalIdentity("telegram", state);
			var userId = await ownerResolver.ResolveOwnerIdAsync(identity, cancel);

			var access = response.Content;
			access.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(access.ExpiresIn);
			await accessStore.SaveAsync(userId, access, cancel);

			return Results.Text("<html><body>Можете закрыть это окно</body></html>", "text/html", Encoding.UTF8, 200);
		}
	}
}