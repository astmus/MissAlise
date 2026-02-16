using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MissAlise.Application;
using MissAlise.Application.Commands;
using MissAlise.DataBase;
using MissAlise.Entities.Identity;
using MissAlise.Interfaces;
using MissAlise.OneDrive;
using MissAlise.OneDrive.Auth;
using MissAlise.TelegramBot;
using MissAlise.TelegramBot.Building;
using MissAlise.TelegramBot.Commands;
using MissAlise.ValueObjects;
using MissAlise.Workflow.Demo.BackgroundSync;
using MissAlise.Workflow.Demo.BotAttrPlayground;
using StackExchange.Redis;

namespace MissAlise.WebApi;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.AddServiceDefaults();

		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
		});

		var azureSection = builder.Configuration.GetSection("AzureAd");
		var botSection = builder.Configuration.GetSection("BotConfiguration");

		builder.Services.AddProblemDetails()
			.AddApplicationServices()
			.AddMigrationsService()
			.AddPersistanceServices(builder.Configuration)
			//.AddBackgroundServer<MissAliseBackgroundServer>()
			//.AddBackgroundJob<SyncOneDriveJob, SyncOneDriveJobHandler>(
			//		builder => builder.SetDescription("Синхронизация OneDrive"))//.AddTrigger(new SyncOneDriveFolderJob(default,default), "1 min").SetDelay(Time.Minute))
			.AddOneDriveService(azureSection)
			.AddBot(botSection, b => {
				b
				.AddCommand<StartCommand>("start", "restart")
				.AddCommand<SyncCommand>("sync", "run sync full/deff", "Синхронизация")
				.AddCommand<MyCommand>("my", "Тестовая команда", "Teстировать")
				.AddCommand<AllInOneDemoCommand>("all", "Тестовая команда", "В одном")
				.AddCommand<BotAttributesPlaygroundCommand>("attr", "attributes test", "Тест атрибутов")
				.AddCommand<BackgroundSyncCommand>("back_sync", "back sync", "Фоновое выполнение")
				.BeginScope<OneDriveRoot>("onedrive", "one drive commands", "Onedrive облако")
					.AddCommand<OneDriveStatus>("status", "status of client")
					.AddCommand<OneDriveAuth>("authorization", "run one drive reauthorization")
						.BeginSection<OneDriveSync>("sync", "sync  scope")
							.AddCommand<OneDriveSyncDiff>("diff", "List")
							.AddCommand<OneDriveSyncFull>("full", "List")
						.EndSection()
				.EndScope<OneDriveRoot>()
				.BeginScope<MediaRoot>("media", "actions with all media items")
					.AddCommand<SettingsSearch>("settings", "search")
				.EndScope<MediaRoot>();
				
			})				
			.AddRouting(options =>
			{
				options.LowercaseUrls = true;
				options.LowercaseQueryStrings = true;
			})
			.AddSingleton<IConnectionMultiplexer>(sp =>
			{
				var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("cache");
				if (string.IsNullOrWhiteSpace(cs))
					throw new InvalidOperationException("ConnectionStrings:cache is missing. In AppHost add .WithReference(cache) to webapi.");
				return ConnectionMultiplexer.Connect(cs);
			})
			.AddAutoMapper(cfg =>
			{
				cfg.CreateMap<Telegram.Bot.Types.User, UserProfile>();
			});


		builder.Services.AddEndpointsApiExplorer(); // только для minimal api
		builder.Services.AddSwaggerGen(options=> {
			var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		});

		var app = builder.Build();
		
		app.Services.UseBotWorkflow();

		app.MapGet("/signin-oidc", Signin);

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseEndpoints(endpoints => endpoints.MapControllers());

		await app.RunAsync();
	}

	[ApiExplorerSettings(IgnoreApi = true)]
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
		await accessStore.SaveAsync(new UserId(userId), access, cancel);
		var bot = ctx.RequestServices.GetRequiredService<Bot>();

		return Results.Text("<html><body>Можете закрыть это окно</body></html>", "text/html", Encoding.UTF8, 200);
	}
}
