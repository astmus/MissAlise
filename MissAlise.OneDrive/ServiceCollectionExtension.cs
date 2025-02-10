using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
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
					PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower

				})
			};

			services.AddRefitClient<IOneDriveCredentialsService>().ConfigureHttpClient(client => client.BaseAddress = new Uri("https://login.microsoftonline.com"));
			services.AddRefitClient<IOneDriveClient>().ConfigureHttpClient(client => client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/me/drive")).AddHttpMessageHandler<AuthHeaderHandler>();
			//services.AddScoped(sp => sp.GetRequiredService<IHandleContext>().GetCurrent<UserProfile>());
			services.AddScoped<TokenCredential, OAuthTokenCredentials>().AddScoped<IOneDriveService, OneDriveService>()
						.AddSingleton(sp => appConfiguration.Get<AzureAd>())
						.AddTransient<AuthHeaderHandler>();
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


		public AuthHeaderHandler(TokenCredential tokenProvider)
		{
			this.tokenCredential = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
			// InnerHandler must be left as null when using DI, but must be assigned a value when
			// using RestService.For<IMyApi>
			// InnerHandler = new HttpClientHandler();
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var token = await tokenCredential.GetTokenAsync(default, cancellationToken);

			//potentially refresh token here if it has expired etc.
			var auth = AuthenticationHeaderValue.Parse(token.Token);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
			//request.Headers.Add("X-Tenant-Id", tokenCredential.GetTenantId());

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
