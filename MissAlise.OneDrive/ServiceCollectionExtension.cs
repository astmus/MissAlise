using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using Refit;

namespace MissAlise.OneDrive
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddOneDriveService(this IServiceCollection services, IConfiguration appConfiguration)
		{
			var settings = new RefitSettings()
			{
				ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
					PropertyNameCaseInsensitive = true
				})				
			};

			services.AddScoped<TokenCredential, OAuthTokenCredentials>().AddScoped<IOneDriveService, OneDriveService>()
						.AddSingleton(sp => appConfiguration.Get<AzureAd>())
						.AddTransient<AuthHeaderHandler>();

			services.AddRefitClient<IOneDriveCredentialsService>(settings).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com"));
			services.AddRefitClient<IOneDriveClient>(settings).ConfigureHttpClient(client => client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/me/drive")).AddHttpMessageHandler<AuthHeaderHandler>();
			//services.AddScoped(sp => sp.GetRequiredService<IHandleContext>().GetCurrent<UserProfile>());
			//.AddScoped<GraphServiceClient>();
			
			//services.AddScoped(sp =>
			//{
			//	var http = GraphClientFactory.Create(sp.GetRequiredService<TokenCredential>());
			//	//sp.GetRequiredService<IHandleContext>().Items.Set(http); возможно наступит момент когда нужен будет сконфитгурированный http но без graphclientservice
			//	return ActivatorUtilities.CreateInstance<GraphServiceClient>(sp, http);
			//});
			//(sp
			//		=> sp.GetRequiredService<IConfiguration>().GetSection(nameof(AzureAd)).Get<AzureAd>());
			return services;
		}
	}

	class AuthHeaderHandler : DelegatingHandler
	{
		private readonly TokenCredential tokenCredential;
		private readonly AzureAd config;
		private readonly IOneDriveCredentialsService credentialService;
		private readonly IUserProfilesRepository profiles;
		private readonly ILogger<AuthHeaderHandler> logger;

		public AuthHeaderHandler(AzureAd config, IOneDriveCredentialsService credentialService, IUserProfilesRepository profiles, ILogger<AuthHeaderHandler> logger)
		{
			//this.tokenCredential = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
			this.config = config;
			this.credentialService = credentialService;
			this.profiles = profiles;
			this.logger = logger;
			// InnerHandler must be left as null when using DI, but must be assigned a value when
			// using RestService.For<IMyApi>
			// InnerHandler = new HttpClientHandler();
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try
			{
				if (request.Options.TryGetValue(new("ctx"), out IHandleContext ctx))
				{
					request.Options.Set(new HttpRequestOptionsKey<string>("ctx"), default);
					//var token = await tokenCredential.GetTokenAsync(default, cancellationToken);
					var profile = await profiles.FindAsync(ctx.GetCurrent<AppUser>().Id, cancellationToken);

					if (DateTimeOffset.UtcNow > profile.AccessData.ExpiredAfter)
					{
						var response = await credentialService.RefreshCredentials(config, profile.AccessData.RefreshToken, cancellationToken).ConfigureAwait(false);
						if (response.IsSuccessful)
						{
							profile.AccessData = response.Content;
							profile.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
							await profiles.AddOrReplaceAsync(profile, cancellationToken);
						}
					}
					var access = new AccessToken(profile.AccessData.AccessToken, profile.AccessData.ExpiredAfter);					
					request.Headers.Add("Authorization", "Bearer "+ profile.AccessData.AccessToken);
				}

			}
			catch (Exception error)
			{
				logger.LogError(error, error.Message);
			}

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
