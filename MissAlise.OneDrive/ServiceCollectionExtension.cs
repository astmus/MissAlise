using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MissAlise.OneDrive;
using Refit;

namespace MissAlise.Bot
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddOneDrive(this IServiceCollection services, IConfiguration appConfig)
		{
			var settings = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions() 
				{
					PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
				})
			};
			services.AddRefitClient<IOneDriveApi>(settings).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com") );
			services.AddScoped(sp => sp.GetRequiredService<IHandleContext>().GetCurrent<UserProfile>());
			services.AddScoped<TokenCredential, OAuthTokenCredentials>().AddScoped<IRemoteStorageService, OneDriveRemoteStorageService>();
			services.AddScoped<GraphServiceClient>(SP=>ActivatorUtilities.CreateInstance<GraphServiceClient>(SP));
			return services;
		}
	}
}
