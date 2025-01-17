using Azure.Core;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.OneDrive
{
	class OAuthTokenCredentials : TokenCredential
	{
		private readonly UserProfile profile;
		private readonly IOneDriveCredentialsService credentialService;
		private readonly AzureAd config;
		private readonly IUserProfilesRepository repository;

		public OAuthTokenCredentials(UserProfile profile, IOneDriveCredentialsService credentialService, AzureAd config, IUserProfilesRepository repository)
		{
			this.profile = profile;
			this.credentialService = credentialService;
			this.config = config;
			this.repository = repository;
		}

		public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
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
			return new AccessToken(profile.AccessData.AccessToken, DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExpiresIn), DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExtExpiresIn), profile.AccessData.TokenType);
		}
	}
}
