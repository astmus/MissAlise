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
			var settings = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
					PropertyNameCaseInsensitive = true
				})				
			};
			var settings2 = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					PropertyNameCaseInsensitive = true
				})
			};

			services.AddScoped<IOneDriveService, OneDriveService>()
						.AddScoped<TokenCredential, OneDriveTokenProvider>()
						.AddTransient<AuthHeaderHandler>().AddOptions<AzureAd>().Bind(azureSection);

			services.AddHttpClient("onedrive");
			services.AddRefitClient<IOneDriveTokenService>(sp=>settings,"onecredentials").ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com"));
			services.AddRefitClient<IOneDriveClient>(settings2).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/me/drive")).AddHttpMessageHandler<AuthHeaderHandler>().AddDefaultLogger();		
			return services;
		}
	}
}
