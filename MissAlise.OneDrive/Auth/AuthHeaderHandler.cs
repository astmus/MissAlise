using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.OneDrive.Auth
{
	class AuthHeaderHandler : DelegatingHandler
	{
		private readonly AzureAd config;
		private readonly IOneDriveTokenService tokenService;
		private readonly IUserProfilesRepository profiles;
		private readonly ILogger<AuthHeaderHandler> logger;

		public AuthHeaderHandler(IOptions<AzureAd> config, IOneDriveTokenService tokenService, IUserProfilesRepository profiles, ILogger<AuthHeaderHandler> logger)
		{
			this.config = config.Value;
			this.tokenService = tokenService;
			this.profiles = profiles;
			this.logger = logger;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try
			{
				if (request.Options.TryGetValue(new("ctx"), out IHandleContext ctx) && request.Options is IDictionary<string, object?> options)
				{					
					options["ctx"] = null;
					var userId = ctx.GetCurrent<AppUser>().Id;
					var profile = await profiles.FindAsync(userId, cancellationToken);

					if (DateTimeOffset.UtcNow > profile.AccessData.ExpiredAfter)
					{
						var response = await tokenService.RefreshCredentials(config, profile.AccessData.RefreshToken, cancellationToken).ConfigureAwait(false);
						if (response.IsSuccessful)
						{
							profile.AccessData = response.Content;
							profile.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
							await profiles.AddOrReplaceAsync(profile, cancellationToken);
						}
					}					
					request.Headers.Add("Authorization", "Bearer "+ profile.AccessData.AccessToken);
				}
			}
			catch (Exception error)
			{
				logger.LogError(error, error.Message);
				return default;
			}

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
