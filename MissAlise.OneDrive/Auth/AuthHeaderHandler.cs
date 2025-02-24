using Microsoft.AspNetCore.Identity;
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
		private readonly IOneDriveTokenService _oneDrive;
		private readonly ILogger<AuthHeaderHandler> logger;
		private readonly UserManager<AppUser> _manager;

		public AuthHeaderHandler(IOptions<AzureAd> config, IOneDriveTokenService oneDrive, ILogger<AuthHeaderHandler> logger, UserManager<AppUser> manager)
		{
			this.config = config.Value;
			this.logger = logger;
			_oneDrive = oneDrive;
			_manager = manager;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try
			{
				if (request.Options.TryGetValue(new("ctx"), out IHandleContext ctx) && request.Options is IDictionary<string, object?> options)
				{
					options.Remove("ctx");
					var appUser = ctx.CurrentUser;
					if (appUser == null)
						return default;

					if (appUser.HasExpiredCredentials())
					{
						var response = await _oneDrive.RefreshCredentials(config, appUser.AccessData.RefreshToken, cancellationToken).ConfigureAwait(false);
						if (response.IsSuccessful)
						{
							appUser.AccessData = response.Content;
							appUser.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
							await _manager.UpdateAsync(appUser).ConfigureAwait(false);
						}
					}

					request.Headers.Add("Authorization", "Bearer " + appUser.AccessData.AccessToken);					
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
