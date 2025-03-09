using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Http.HttpClientLibrary;
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

			services.AddScoped<IOneDriveService, OneDriveService>()
						.AddScoped<TokenCredential, OneDriveTokenProvider>()
						.AddOptions<AzureAd>().Bind(azureSection);

			services.AddHttpClient<ApiClient>().AddTypedClient((httpClient, sp) =>
				{
					var credentials = sp.GetRequiredService<TokenCredential>();
					var options = sp.GetRequiredService<IOptions<AzureAd>>().Value;
					var authProvider = new AzureIdentityAuthenticationProvider(credentials, scopes: options.Scopes.Split(' '));
					var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
					{ 
						BaseUrl = "https://graph.microsoft.com/v1.0"
					};

					return new ApiClient(requestAdapter);
				})
				.ConfigurePrimaryHttpMessageHandler(_ =>
				{
					var defaultHandlers = KiotaClientFactory.CreateDefaultHandlers();
					var defaultHttpMessageHandler = KiotaClientFactory.GetDefaultHttpMessageHandler();

					return KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink(
						defaultHttpMessageHandler, [.. defaultHandlers])!;
				});

			services.AddRefitClient<IOneDriveTokenService>(snakeCase).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com"));
			return services;
		}
	}
}
