using Microsoft.Extensions.Options;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using Azure.Core;
using MissAlise.Interfaces;
using MissAlise.Application.Models;

namespace MissAlise.OneDrive.Auth
{
	internal class OneDriveTokenProvider : TokenCredential
	{
		private readonly IOneDriveTokenService credentialService;
		private readonly AzureAd config;
		private readonly IUserProfilesRepository repository;
		private readonly IHandleContext ctx;
		private UserProfile profile;

		public OneDriveTokenProvider(IOneDriveTokenService credentialService, IOptions<AzureAd> config, IUserProfilesRepository repository, IHandleContext ctx)
		{
			this.credentialService = credentialService;
			this.config = config.Value;
			this.repository = repository;
			this.ctx = ctx;
		}

		public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			var appUser = ctx.GetCurrent<AppUser>();
			profile = await repository.FindAsync(appUser.Id.ToString(), cancellationToken);
			if (profile == null)
				return default;	

			if (DateTimeOffset.UtcNow > profile.AccessData.ExpiredAfter)
			{
				var response = await credentialService.RefreshCredentials(config, profile.AccessData.RefreshToken, cancellationToken).ConfigureAwait(false);
				if (response.IsSuccessful)
				{
					profile.AccessData = response.Content;
					profile.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
					_ = repository.AddOrReplaceAsync(profile, cancellationToken);
				}
			}
			return new AccessToken(profile.AccessData.AccessToken, profile.AccessData.ExpiredAfter);
		}

		public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			return new AccessToken(profile.AccessData.AccessToken, DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExpiresIn), DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExpiresIn), profile.AccessData.TokenType);
		}
	}
}
