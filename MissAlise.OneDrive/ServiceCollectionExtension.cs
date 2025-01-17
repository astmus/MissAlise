using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using Refit;

namespace MissAlise.OneDrive
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddOneDriveService(this IServiceCollection services, IConfigurationSection azureConfigurationSection)
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
			services.AddScoped<TokenCredential, OAuthTokenCredentials>().AddScoped<IOneDriveService, OneDriveService>()
						.AddSingleton<AzureAd>().Configure<AzureAd>(azureConfigurationSection);

			services.AddScoped(sp =>
			{
				var http = GraphClientFactory.Create(ActivatorUtilities.CreateInstance<OAuthTokenCredentials>(sp));
				//sp.GetRequiredService<IHandleContext>().Items.Set(http); возможно наступит момент когда нужен будет сконфитгурированный http но без graphclientservice
				return ActivatorUtilities.CreateInstance<GraphServiceClient>(sp, http);
			});
			//(sp
			//		=> sp.GetRequiredService<IConfiguration>().GetSection(nameof(AzureAd)).Get<AzureAd>());
			return services;
		}
	}
}
