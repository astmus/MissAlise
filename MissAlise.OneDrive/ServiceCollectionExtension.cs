using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive.Auth;
using Refit;

namespace MissAlise.OneDrive
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddOneDriveService(this IServiceCollection services, IConfiguration azureSection)
		{
			var snakeCase = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
					PropertyNameCaseInsensitive = true
				})				
			};

			var camelCase = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					PropertyNameCaseInsensitive = true
				})
			};

			services.AddScoped<IOneDriveService, OneDriveService>()
						.AddScoped<TokenCredential, OneDriveTokenProvider>()
						.AddOptions<AzureAd>().Bind(azureSection);

			services.AddHttpClient("onedrive");
			services.AddRefitClient<IOneDriveTokenService>(sp=>snakeCase,"onecredentials").ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com"));
			services.AddRefitClient<IOneDriveClient>(camelCase).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/me/drive")).AddDefaultLogger();		
			return services;
		}
	}
}
