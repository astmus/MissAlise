using Azure.Core;
using Microsoft.Extensions.Options;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.Identity;
using MissAlise.Interfaces;

namespace MissAlise.OneDrive.Auth
{
	internal class OneDriveTokenProvider : TokenCredential
	{
		private readonly IOneDriveTokenService credentialService;
		private readonly AzureAd config;
		private readonly IHandleContext ctx;
		private readonly IAccessCredentialsStore _accessStore;
		private SyncPrincipal? _principal;

		public OneDriveTokenProvider(IOneDriveTokenService credentialService, IOptions<AzureAd> config, IHandleContext ctx, IAccessCredentialsStore accessStore)
		{
			this.credentialService = credentialService;
			this.config = config.Value;
			this.ctx = ctx;
			_accessStore = accessStore;
		}

		public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			_principal = ctx.Get<SyncPrincipal>();
			if (_principal is null)
				return default;

			var access = _principal.Access;
			if (access.IsExpired())
			{
				var response = await credentialService.RefreshCredentials(config, access.RefreshToken, cancellationToken).ConfigureAwait(false);
				if (response.IsSuccessful)
				{
					var updated = response.Content;
					updated.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(updated.ExpiresIn);
					await _accessStore.SaveAsync(_principal.Profile.UserId, updated, cancellationToken).ConfigureAwait(false);
					_principal = _principal with { Access = updated };
					access = updated;
				}
			}
			return new AccessToken(access.AccessToken, access.ExpiredAfter);
		}

		public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
		{
			if (_principal?.Access is not { } access)
				return default;
			return new AccessToken(access.AccessToken, access.ExpiredAfter);
		}
	}
}
