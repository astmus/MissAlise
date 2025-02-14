using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.OneDrive
{
	class OAuthTokenCredentials : TokenCredential 
	{		
		private readonly IOneDriveCredentialsService credentialService;
		private readonly AzureAd config;
		private readonly IUserProfilesRepository repository;
		private readonly IHandleContext ctx;		

		UserProfile profile 
			=> ctx.GetCurrent<UserProfile>();

		public OAuthTokenCredentials(IOneDriveCredentialsService credentialService, AzureAd config, IUserProfilesRepository repository)
		{			
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
			return new AccessToken(profile.AccessData.AccessToken, DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExpiresIn), DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExpiresIn), profile.AccessData.TokenType);
		}
	}
}
