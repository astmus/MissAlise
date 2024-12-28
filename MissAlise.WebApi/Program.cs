using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using MissAlise.WebApi.Auth;

namespace MissAlise.WebApi;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.AddServiceDefaults();

		builder.Logging.AddConsole();

		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;			
		});

		builder.Services.Configure<AzureConfiguration>(builder.Configuration.GetSection("AzureAd"));

		builder.Services.AddSingleton<GraphServiceClient>(serviceProvider =>
		{
			var azureConfig = serviceProvider.GetRequiredService<IOptions<AzureConfiguration>>().Value;
			var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
			{
				ClientId = azureConfig.ClientId,
				TenantId = azureConfig.TenantId
			});
			return new GraphServiceClient(credential, azureConfig.GetScopes());
		});

		var app = builder.Build();

		app.UseAuthentication();
		app.UseAuthorization();		

		app.UseHttpsRedirection();		

		app.UseRouting();

		app.MapControllers();

		//app.MapDefaultEndpoints();

		//// Configure the HTTP request pipeline.

		//app.UseHttpsRedirection();

		//app.UseAuthentication();
		//app.UseAuthorization();

		//app.MapControllers();

		app.Run();
	}
}
