using Microsoft.Extensions.Options;
using MissAlise.Application.Interfaces;
using MissAlise.Application;
using MissAlise.Entities.OneDrive;
using Azure.Core;
using MissAlise.Interfaces;
using Microsoft.AspNetCore.Identity;
using MissAlise.Application.Common;

namespace MissAlise.OneDrive.Auth
{
	internal class OneDriveTokenProvider : TokenCredential
	{
		private readonly IOneDriveTokenService credentialService;
		private readonly AzureAd config;
		private readonly UserManager<AppUser> _manager;
		private readonly IHandleContext ctx;
		private AppUser appUser;

		public OneDriveTokenProvider(IOneDriveTokenService credentialService, IOptions<AzureAd> config, UserManager<AppUser> manager, IHandleContext ctx)
		{
			this.credentialService = credentialService;
			this.config = config.Value;
			this._manager = manager;
			this.ctx = ctx;
		}

		public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			appUser = ctx.CurrentUser;
			if (appUser == null)
				return default;

			if (appUser.HasExpiredCredentials())
			{
				var response = await credentialService.RefreshCredentials(config, appUser.AccessData.RefreshToken, cancellationToken).ConfigureAwait(false);
				if (response.IsSuccessful)
				{
					appUser.AccessData = response.Content;
					appUser.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
					await _manager.UpdateAsync(appUser).ConfigureAwait(false);
				}
			}
			return new AccessToken(appUser.AccessData.AccessToken, appUser.AccessData.ExpiredAfter);
		}

		public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			return new AccessToken(appUser.AccessData.AccessToken, DateTimeOffset.FromUnixTimeSeconds(appUser.AccessData.ExpiresIn), DateTimeOffset.FromUnixTimeSeconds(appUser.AccessData.ExpiresIn), appUser.AccessData.TokenType);
		}
	}
}
