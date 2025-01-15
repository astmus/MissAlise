using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using Refit;

namespace MissAlise.OneDrive
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddOneDrive(this IServiceCollection services, IConfiguration appConfig)
		{
			var settings = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
				})
			};
			
			services.AddRefitClient<IOneDriveCredentialsService>(settings).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com"));
			services.AddScoped(sp => sp.GetRequiredService<IHandleContext>().GetCurrent<UserProfile>());
			services.AddScoped<TokenCredential, OAuthTokenCredentials>().AddScoped<IRemoteStorageService, OneDriveService>();
			services.AddScoped(sp =>
			{
				var http = GraphClientFactory.Create(ActivatorUtilities.CreateInstance<OAuthTokenCredentials>(sp));
				sp.GetRequiredService<IHandleContext>().Items.Set(http);
				return ActivatorUtilities.CreateInstance<GraphServiceClient>(sp,http);
			});
			return services;
		}
	}
}
