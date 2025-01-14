using Azure.Core;
using MissAlise.Entities.OneDrive;

namespace MissAlise.Bot
{
	class OAuthTokenCredentials : TokenCredential
	{
		private readonly UserProfile profile;

		public OAuthTokenCredentials(UserProfile profile)
		{
			this.profile = profile;
		}
		public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			await Task.CompletedTask;

			return new AccessToken(profile.AccessData.AccessToken, DateTimeOffset.Now.AddSeconds(profile.AccessData.ExpiresIn));
		}
		public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			return new AccessToken(profile.AccessData.AccessToken, DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExpiresIn), DateTimeOffset.FromUnixTimeSeconds(profile.AccessData.ExtExpiresIn), profile.AccessData.TokenType);
		}
	}
}
