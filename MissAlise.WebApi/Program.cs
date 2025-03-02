using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MissAlise.Application;
using MissAlise.Application.Background;
using MissAlise.Application.Background.Handlers;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Services.User;
using MissAlise.Background;
using MissAlise.Bot;
using MissAlise.DataBase;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive;
using MissAlise.OneDrive.Auth;
using MissAlise.Worker.Background;

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
			.AddBackgroundServer<MissAliseBackgroundServer>()
			.AddBackgroundJob<SyncOneDriveJob, SyncOneDriveJobHandler>(
					builder => builder.SetDescription("Синхронизация OneDrive"))//.AddTrigger(new SyncOneDriveFolderJob(default,default), "1 min").SetDelay(Time.Minute))
			.AddOneDriveService(azureSection)			
			.AddBotService(botSection)
			.AddRouting(options =>
			{
				options.LowercaseUrls = true;
				options.LowercaseQueryStrings = true;
			});

		//builder.Services.AddEndpointsApiExplorer(); это только для minimal api
		//builder.Services.AddSwaggerGen(options=> {
		//	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		//	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		//}
		//);
		ApplyMapping(builder.Services);
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
		app.UseEndpoints(endpoints => endpoints.MapControllers());

		await app.RunAsync();
	}

	static async Task<IResult> Signin([FromServices] IOptions<AzureAd> options, [FromQuery] string code, [FromQuery] string state, HttpContext ctx, CancellationToken cancel)
	{
		AzureAd config = options.Value;
		if (ctx.Request.Headers.Referer.Any(refer => refer == config.Instance || refer == "https://login.live.com/" || refer == "https://account.live.com/") == false)
			return Results.Forbid();
		
		var response = await ctx.RequestServices.GetRequiredService<IOneDriveTokenService>().GetCredentialsByCode(config, code, cancel);

		if (!response.IsSuccessful)
			return Results.Problem(response.Error.ToString(), null, (int)response.StatusCode);
		UserManager<AppUser> manager = ctx.RequestServices.GetRequiredService<UserManager<AppUser>>();
		//var userSrv = ctx.RequestServices.GetRequiredService<IUserService>();
		var appUser = await manager.Users.Include(user => user.AccessData).SingleOrDefaultAsync(user => user.Id == state);
		appUser.AccessData = null;
		var result = await manager.UpdateAsync(appUser);
		
		appUser.AccessData = response.Content;
		appUser.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
		appUser.EmailConfirmed = true;	
		result = await manager.UpdateAsync(appUser);
		if (result.Succeeded)
			return Results.Text("<html><body>Авторизация закончена успешно. Вы можете закрыть это окно</body></html>", "text/html", Encoding.UTF8, 200);
		else
			return Results.Text($"<html><body>Ошибка авторизации. {string.Join('\n', result.Errors.Select(sel=> sel.Description))}</body></html>", "text/html", Encoding.UTF8, 401);
	}

	static void ApplyMapping(IServiceCollection services)
	{
		// use DI (http://docs.automapper.org/en/latest/Dependency-injection.html) or create the mapper yourself
		services.AddAutoMapper(cfg =>
		{
			cfg.CreateMap<Telegram.Bot.Types.User, AppUser>();
			//cfg.CreateMap<Bar, BarDto>();
		});
	}
}
