using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Graph;
using MissAlise.WebApi.Auth;

namespace MissAlise.WebApi;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
		builder.AddServiceDefaults();

		builder.Logging.AddConsole();

		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
		});

		builder.Services.Configure<AzureConfiguration>(builder.Configuration.GetSection("AzureAd"));
		var config = builder.Configuration.GetSection("AzureAd").Get<AzureConfiguration>();
		builder.Services.AddTransient<DelegateAuthenticationProvider2>();
		builder.Services.AddScoped<GraphServiceClient>(sp =>
		{
			return new GraphServiceClient(sp.GetRequiredService<DelegateAuthenticationProvider2>());
		});	
			
		builder.Services.AddAuthorization();


		// Добавление сервисов для использования Microsoft Graph API
		//builder.Services.AddScoped<GraphServiceClient>();

		//builder.Services.AddSingleton<GraphServiceClient>(serviceProvider =>
		//{
		//	var azureConfig = serviceProvider.GetRequiredService<IOptions<AzureConfiguration>>().Value;

		//	var app = ConfidentialClientApplicationBuilder.Create(azureConfig.ClientId)
		//	.WithClientSecret(azureConfig.ClientSecret)
		//	.WithAuthority(new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token"))
		//	.Build();
		//	var cacheFilePath = "D:\\Temp\\cache2.txt";
		//	// Настройка кэширования с чтением и записью в файл
		//	var tokenCache = app.AppTokenCache;
		//	tokenCache.SetBeforeAccess(args =>
		//	{
		//		if (File.Exists(cacheFilePath))
		//		{
		//			args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(cacheFilePath));
		//		}
		//	});

		//	tokenCache.SetAfterAccess(args =>
		//	{
		//		if (args.HasStateChanged)
		//		{
		//			File.WriteAllBytes(cacheFilePath, args.TokenCache.SerializeMsalV3());
		//		}
		//	});
		//	//var cacheHelper = new MsalCacheHelper(new StorageCreationProperties("myapp_token_cache.dat","D:\\"));
		//	var authenticationProvider = new DelegateAuthenticationProvider();

		//	GraphServiceClient client = new GraphServiceClient(authenticationProvider);

		//	return client;
		//});

		var app = builder.Build();

		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();

		app.UseHttpsRedirection();


		app.MapControllers();

		app.MapDefaultEndpoints();

		//// Configure the HTTP request pipeline.

		//app.UseHttpsRedirection();

		//app.UseAuthentication();
		//app.UseAuthorization();

		//app.MapControllers();

		app.Run();
	}
}
