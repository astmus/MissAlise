using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MissAlise.Application;
using MissAlise.Application.Interfaces;
using MissAlise.Background;
using MissAlise.Bot;
using MissAlise.DataBase;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive;
using MissAlise.OneDrive.Auth;
using MissAlise.Utils;
using MissAlise.Worker.Background;
using MissAlise.Worker.Background.Handlers;

namespace MissAlise.WebApi;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
		builder.AddServiceDefaults();

		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;			
		});

		var azureSection = builder.Configuration.GetSection("AzureAd");
		var botSection = builder.Configuration.GetSection("BotConfiguration");		
		
		builder.Services.AddProblemDetails();		
		builder.Services.AddApplication()
			.AddMigrateService()
			.AddPersistance(builder.Configuration)
			.AddBackgroundServer<MissAliseBackgroundServer>()
				//.AddBackgroundJob<SyncDataJob, SyncBackgroundTaskHandler>(
				//	builder => builder.SetDescription("Синхронизация данных").AddTrigger(new SyncDataJob(64), "Полуминутно").SetDelay(Time.Minute / 2))
				.AddBackgroundJob<SyncOneDriveJob, SyncOneDriveJobHandler>(
					builder => builder.SetDescription("Синхронизация OneDrive")//.AddTrigger(new SyncOneDriveFolderJob(default,default), "1 min").SetDelay(Time.Minute)
				)
			.AddOneDriveService(azureSection)
			.AddOneDriveHandling()
			.AddBotService(botSection);
		
		//builder.Services.AddEndpointsApiExplorer(); это только для minimal api
		//builder.Services.AddSwaggerGen(options=> {
		//	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		//	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		//}
		//);

		var app = builder.Build();
		
		app.MapGet("/signin-oidc", Signin);

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			//app.UseSwagger();
			//app.UseSwaggerUI();
		}		
		
		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});

		await app.RunAsync();
	}

	static async Task<IResult> Signin([FromServices] AzureAd config, [FromServices] HttpClient client, [FromQuery] string code, [FromQuery] string state, HttpContext ctx, CancellationToken cancel)
	{
		if (ctx.Request.Headers.Referer.Any(refer => refer == config.Instance || refer == "https://login.live.com/" || refer == "https://account.live.com/") == false)
			return Results.Forbid();
		//var response2 = await ctx.RequestServices.GetRequiredService<IOneDriveCredentialsService>().GetCredentialsByCode2(config, code, cancel);
		var response = await ctx.RequestServices.GetRequiredService<IOneDriveTokenService>().GetCredentialsByCode(config, code, cancel);

		if (!response.IsSuccessful)
			return Results.Problem(response.Error.ToString(), null, (int)response.StatusCode);

		await ctx.RequestServices.GetService<IAuthorizationCompleter>()?.AuthorizationCompleted(state, response.Content, cancel);
		return Results.Text("<html><body>Авторизация закончена успешно. Вы можете закрыть это окно</body></html>", "text/html", Encoding.UTF8, 200);
	}
}
